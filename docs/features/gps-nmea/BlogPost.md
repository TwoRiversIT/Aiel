# From Hero to Zero: How I wrote a GPS Parser Four Times

*This is the story of how a tiny hobby parser turned into a multi‑million‑messages‑per‑second monster, and how I accidentally learned more about .NET performance than I ever intended.*

You can skip to the ========= for the good stuff. (IYKYK)

## v1 - Just For Fun

About 10 years ago I wrote a [NMEA](https://en.wikipedia.org/wiki/NMEA_0183) sentence parser. I had purchased a little u-blox USB GPS receiver and had some time to kill. I got my head around the NMEA message format and wrote a parser. No unit tests, No performance tests. No git repo. No README! I actually had more fun learning how to read data from the serial port. After all the years of [Telix](https://en.wikipedia.org/wiki/Telix), Bulletin Boards and ASCII art, I finally understood what N-8-1 actually meant!

And the bonus was that I now knew where my desk was at any given time!

## v2 - Maybe Some Profit?

In 2019 a friend asked me if I was interested in helping him with a GPS project. It turned out that my friend had a friend, who worked for an Alberta company that wanted a portable system for use waaaay up north, that could scan gas pipelines for gas leaks, from the air while zipping around oil patches in helicopters! They even teased me saying I might get to fly with them sometime. The project sounded like a lot fun, and I already knew everything there was to know about GPS!

I dusted off v1, fired it up, and realized that it was way too clunky to run on a Raspberry Pi, especially when paired with Leaflet and OpenStreetMap. So after some "Googling with Bing" (Hi Mr. Hanselman!), I devoured some blog posts and got started with a rewrite.

In my research I discovered the new hotness was `Span<T>`, `Memory<T>`, `ReadOnlySequence<T>`, and Pipelines. Heavy lifting for my meager brain, but it seemed like a perfect fit! I had also been following [Robert Nystrom](https://x.com/intent/user?screen_name=munificentbob) as he was writing his book [Crafting Interpreters](https://www.craftinginterpreters.com/), where I discovered the brain exploding world of Lexers. As an avid ready of Stephen Cleary's blog, I also discovered the [Producer/Consumer](https://blog.stephencleary.com/2012/11/async-producerconsumer-queue-using.html) pattern and recognized it as a bright shiny new hammer for my GPS nails.

I worked away at the rewrite for a few weeks. I learned a lot, cried a bit, then learned a lot more. In the end I was especially proud of how I managed to make it extensible so that every other programmer in the world could painlessly add their own NMEA sentences. They would just write a class like this:

```csharp
public class GLL : NmeaMessage
{
    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$GPGLL").AsMemory();
    protected override ReadOnlyMemory<Byte> Key => KEY;

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = new Lexer(sentence);

        if (lexer.NextString() != "GPGLL")
        {
            throw lexer.Error();
        }

        return new GLL()
        {
            Latitude = lexer.NextLatitude(),
            Longitude = lexer.NextLongitude(),
            FixTime = lexer.NextTimeSpan(),
            DataActive = lexer.NextChar(),
            Checksum = lexer.NextChecksum()
        };
    }

    public Double Latitude { get; private set; }
    public Double Longitude { get; private set; }
    public TimeSpan FixTime { get; private set; }
    public Char DataActive { get; private set; }

    public override String ToString() => $"GPGLL {Latitude} {Longitude} {FixTime} {DataActive}";
}
```

And then register it like this:

```csharp
var nsr = new NmeaStreamReader();

nsr.Register(new GLL());
```

And then `async` and `await` to their hearts content:

```csharp
await nsr.ParseStreamAsync(stream, async (message) => await DispatchMessage(message), stoppingToken);
```

Voila! It was truly a thing of beauty! I was so proud!

Then the client bailed (I think I took too long). I was disappointed that I did not get to ride in a helicopter, but I had written some good code. I bundled it up, added a README and a sample program and pushed it onto GitHub and NuGet. Then I promptly forgot all about it.

## v3 - The Read Deal

Fast forward to last week when I was 'unlisting' my abandoned NuGet packages and noticed that [DKW.NMEA](https://www.nuget.org/packages/DKW.NMEA) has over 25,000 downloads! Excited, I pulled up the repo and had a look. **1 star**. Probably my mom. Nobody was actually using it. Not even me. I figured that the NuGet downloads must be automated tools that index or analyze that stuff. *deep sigh* No custom NMEA messages. No endlessly filling pipelines draining asynchronously until the heat death of the universe.

I was getting ready to archive the repo but decided to take one last look through the code. I thought to myself, "you know, this was such a joy to build, maybe you should update it to .NET 10!".

I did not really have time to burn, so I enlisted my friend Microsoft Copilot and we threw a couple afternoons at it. It was a lot of fun working together through the update. We found **a lot** of embarrassing bugs, fixed them, added a huge number of tests, and then sat back very pleased with ourselves. Little pat on the back for me, a little thumbs up for Copilot. It was not a true v3 because the API stayed almost identical to v2, but there was a pretty good performance boost!

Well, a little bit of a performance boost, I think? I had never benchmarked v1 or v2, and now I was running v3 on a computer with lots of the gigahurts and a huge amount of RAM that I beat Copilot to. Definitely **not** a Raspberry Pi. The reality was that any perceived performance increase was 100% because of improvements in .NET, not anything I had done.

For kicks, I had GitHub Copilot help me implement a couple BenchmarkDotNet tests because that is what you do. We got some benchys written, but after running them a few times I had **absolutely no idea what I was looking at**. I am pretty sure I graduated high school, but I must have slept through the unit on standard deviation, whatever that is. So back to Microsoft Copilot with a huge copy/paste of logs for an *explain it to me like I am 5* session.

It was great! Copilot helped me get a basic grasp on what I was looking at, and I was able to add some meaningful benchmarks. Some of them with real world data collected from an actual helicopter! Copilot was very encouraging and I came away feeling better about calling this iteration v3. And myself a developer!

Then I made a mistake that cost me a week of sleep and some pretty significant hair loss.

=========

## v4 - The Good Stuff

I asked Microsoft Copilot, "How can I make it better?" I expected a couple of micro‑optimizations and maybe a suggestion to make something `static readonly`. We get a little bump in the benchmarks, and I could once again push it up to Nuget and forget about my little toy. Instead, the earth opened up beneath us and we fell into a week‑long optimization rabbit hole that touched everything: allocation patterns, span‑based parsing, struct‑based lexing, and the cold, hard reality that my “pretty good” v3 parser was leaving *a lot* of performance on the table.

Here is the nitty‑gritty of what actually moved the needle.

---

### 1. Stop Allocating. No, Really. Stop

My v3 parser *felt* efficient because it used `ReadOnlySequence<Byte>` and pipelines, but it still allocated constantly:

- Strings for every field  
- Substrings for every token  
- Temporary buffers for numeric parsing  
- Exceptions for malformed sentences  

At one point Copilot even pointed out my code comment:

```csharp
// I know I should write a custom DateTime parser for this... but I am too lazy.
```

So that is where we started. Adding zero allocation `NextDate()` and `NextTime()` methods and then combining them into a cute little `NextDateTime()` method. That should give us a nice little bump in the benchmarks, right? Nope.

The first breakthrough was brutally simple. If we were going to gain any performance, there had to be **zero allocations, period.**

That meant:

- No `new string(...)`
- No `slice.ToString(Encoding.UTF8)`  
- No exceptions for control flow  
- No boxing  
- No hidden allocations from helper methods

Everything became `ReadOnlySequence<Byte>`, and we deferred the decision of when to materialize strings to the caller.

Yeah, you read that right. `ReadOnlySequence<Byte>`. It turns out I still had `buffer.IsSingleSegment` sprinkled throughout a bunch of extension methods. If `IsSingleSegment` was true, we had a nice happy path and could simply return a string:

```csharp
if (buffer.IsSingleSegment)
{
    return encoding.GetString(buffer.First.Span);
}
```

But if we were not on the happy path there was this garbage to contend with:

```csharp
return String.Create((Int32)buffer.Length, buffer, (span, sequence) =>
{
    foreach (var segment in sequence)
    {
        encoding.GetChars(segment.Span, span);
        span = span[segment.Length..];
    }
});
```

CRAP! Both paths resulted in an allocation!! It was slowly starting to dawn on me that this "Zero Allocation" thing might not be as simple as Copilot had led me to believe.

Getting rid of the sequences so that we could operate on spans was going to have to come first.

### 2. Filling the Pipe is easy. Reading from the pipe is easy, until it isn't

So just like in the extension methods, I was going to have to scan through the linked list of buffers, looking for the start of a sentence, store the position, and then continue forward until I found the end. Once I had that it should be simple enough to grab everything between the start and end and send that into the lexer. Right?

Nope. Not simple. I went around in circles trying to figure out which comes first, the chicken or the egg, the span or the sequence.

Then Microsoft Copilot had the brilliant deduction that we are not really parsing data.

### 3. Replace “Parsing” with “Extraction”

This realization was subtle but transformative: **NMEA parsing is not parsing. It is extraction.**

NMEA sentences are rigid:

```text
$GPGGA,field1,field2,field3,...*checksum
```

**Copilot**: Doug, you do not need a grammar. You do not need a tokenizer. You do not need a state machine.

**Doug**: No state machine? But I love state machines! "Finite State Automata" Even their names are cool! Are you sure we do not need one?

**Copilot**: No. All you really need is:

- Find `$`
- Find `,`
- Find `*`
- Find `\n`

Then slice a few spans.

So we replaced the “parser” with a little bit of code that did exactly three things:

1. Find the start: `$`
2. Find the end: `\n`
3. Return the slice  

No allocations. No interpretation. No ceremony.

With that single change to the mental model we soon arrived at:

```csharp
Boolean TryReadSentence(ref ReadOnlySequence<Byte> buffer, out ReadOnlySequence<Byte> sequence)
```

When we get a `false`, we read more from the pipe and then try again. When we get a `true` we have a full sentence and it is then trivial to turn the segments into a single span:

```csharp
private static ReadOnlySpan<Byte> GetContiguousBytes(ReadOnlySequence<Byte> sequence)
{
    // Happy path: single segment
    if (sequence.IsSingleSegment)
    {
        return sequence.FirstSpan;
    }

    // Sad path: multiple segments, need to copy
    return sequence.ToArray();
}
```

Once we had the raw sentence as a `ReadOnlySpan<Byte>`, the lexer got *a lot* simpler. In fact, it just became a set of helper methods. No tokens, no enums, no AST. Just:

- `Char NextChar()`
- `Boolean NextBoolean()`
- `Int32 NextInteger()`
- `Double NextDouble()`
- `Int32 NextHexadecimal()`

And a little fancier:

- `Double NextLatitude()`
- `Double NextLongitude()`
- `DateOnly NextDate()`
- `TimeOnly NextTime()`

And my favorite:

- `DateTime NextDateTime()`

There was still one more breakthrough to come.

### 4. WTF is a `ref struct`?

**Doug**: This was all pretty straightforward until I got to the part where you said I should convert the Lexer to a `ref struct`. Especially since I have never heard of `ref struct` before. Is it `readonly struct`'s baby brother?

**Copilot**: Totally fair reaction — `ref struct` looks exotic the first time you meet it, but once you understand what it actually is, the whole thing becomes beautifully simple. And in your case, it is exactly the right tool. Let me explain.

A `ref struct` is just a `struct` with one rule:

**It *MUST* live on the stack.**

That is it. No magic.

You already know that the stack is fast and the heap is slow. The compiler allows a `ref struct` on the stack by enforcing its *forbidden rules*:

- no boxing
- no storing it in fields
- no capturing it in lambdas
- no async/await
- no iterators
- no interfaces
- no heap allocation

**Doug**: Uh... ok. Why?

**Copilot**: Because `ref struct` types usually contain spans, and spans point to unmanaged memory. Unmanaged memory is "dangerous", so the compiler guarantees that spans *never* outlive the memory they reference. That is the entire story.

Let me explain.

Your new Lexer holds:

- a `ReadOnlySpan<Byte>`
- an `Int32` cursor

You might not have realized this, but a span is also stack‑only. If you write the Lexer in a way that it does not break any of the *forbidden rules*, it can also live on the stack. This is exactly what `ref struct` is for.

**Doug**: Yeah, but I use C# so I never have to deal with unmanaged anything. It is not safe for a mere mortal like me!

**Copilot**: No really, this is safe!

- The pipeline reader extracts a complete sentence
- It copies it into a contiguous buffer
- That buffer is alive for the duration of the parse
- The Lexer only ever sees a `ReadOnlySpan<Byte>` pointing into that buffer
- The Lexer is a `ref struct`, so it cannot escape the stack
- The compiler enforces lifetime safety

You get C‑level performance with C#‑level safety.

**Doug**: Oh. My. God. *deep breath* Okay. I wanna do that!

---

## Benchmarks That Actually Measure Reality

My first BenchmarkDotNet tests were… naive. Copilot helped me build proper tests:

- Realistic sentence sizes
- Mixed sentence types
- Real-world GPS data (collected from an actual helicopter!)
- Cold vs warm runs
- Memory diagnostics

Once the benchmarks were honest, we could see exactly which changes mattered.

### Single Message Parsing

The first thing I wanted to know: how fast can we parse a single NMEA sentence?

| Version                   | Time      | Memory  | vs Baseline   |
| ------------------------- | --------- | ------- | ------------- |
| v2 (DKW.NMEA)             | 663 ns    | 528 B   | baseline      |
| v3 (Aiel.Gps)        | 648 ns    | 216 B   | 0.98x         |
| **v4 (Aiel.Gps.HP)** | **82 ns** | **0 B** | **8x faster** |

That is not a typo. **Zero bytes allocated.** The `ref struct` Lexer and struct-based message types mean the entire parse happens on the stack.

### Stream Parsing (Real World Data)

But single-message benchmarks do not tell the whole story. What about parsing thousands of messages from a stream, like you would from a real GPS device?

| Dataset             | v2                | v3               | v4 (HP)             | HP vs v2        |
| ------------------- | ----------------- | ---------------- | --------------------| --------------- |
| Small (343 msgs)    | 304 μs / 395 KB   | 503 μs / 216 KB  | **47 μs / 73 KB**   | **6.4x faster** |
| Medium (4,483 msgs) | 4.4 ms / 5.4 MB   | 6.4 ms / 2.6 MB  | **782 μs / 1.1 MB** | **5.7x faster** |
| Large (13,470 msgs) | 12.5 ms / 16.3 MB | 19.1 ms / 7.6 MB | **2.5 ms / 3.2 MB** | **5x faster**   |

A few things jumped out at me:

1. **v3 is actually slower than v2 for stream parsing.** The `IAsyncEnumerable` API is cleaner, but the overhead of the async state machine adds up.

2. **v4 is 5-6x faster across the board.** And that is with real-world, mixed-message data.

3. **Memory allocation dropped by 80%.** From 16.3 MB down to 3.2 MB for the large dataset. The remaining allocations are mostly pipeline infrastructure overhead.

### The Final Math

I let Copilot do the math on that large dataset:

- 13,470 messages in 2.5 ms is...
- ... carry the 1 ...
- **5.4 million messages per second**

On my workstation (Intel Core i9-14900k, nothing special), the final version peaked at *over 5 million NMEA **sentences** per second*.

For a hobby parser, that is ridiculous. And delightful.

---

## The Final Lesson

The biggest surprise was not the performance. It was how much fun it was to tear this thing apart and rebuild it with modern .NET tools, a decade more of experience, and an AI partner who never got tired of reading benchmark logs.

The end result is not just faster. It is cleaner, more explicit, more maintainable, and more honest about what NMEA parsing actually is. And by jumping through a few hoops, it is still extensible. But that is a topic for another day.

And yes, I now know where my desk is again. But this time, I can find it *5.4 million* times per second.

If you want to see all the blood sweat and tears for yourself, the repo is [here](https://github.com/AielIT/AppFramework). There is a bit of other code in there that I have been working on, and a lot more to come. If you cannot tell, I love re-inventing the wheel! PRs, issues, and “Doug, you're an ID10T!” comments are welcome.

---

**Doug Wilson** is a long‑time .NET developer, former photographer, recovering state‑machine enthusiast, and full‑time stay‑at‑home parent who occasionally escapes into code when the stars align. He wrote his first BASIC programs in notebooks, typed them in at school on an Apple ][, copied down the bugs and then did it all over again the next day with the fixes. He enjoys elegant APIs, pizza and espresso, and any excuse to learn something the hard way.

Someone is wrong on the Internet! If it happens to be him, just break it to him gently. He has not had much sleep since the birth of his three children.
