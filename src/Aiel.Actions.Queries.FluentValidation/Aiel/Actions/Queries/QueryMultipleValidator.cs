// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using FluentValidation.Validators;

namespace Aiel.Actions.Queries;

public class QueryMultipleValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : QueryMultiple
{
    public QueryMultipleValidator()
    {
        RuleFor(x => x.Page).NotNull().SetValidator(new PageValidator<TRequest>());
        RuleFor(x => x.Sort).NotNull().SetValidator(new SortValidator<TRequest>());
    }
}

public class SortValidator<T> : IPropertyValidator<T, SortOrder>
    where T : QueryMultiple
{
    public String Name => "Sort Order";

    public String GetDefaultMessageTemplate(String errorCode)
    {
        return "{PropertyName} is invalid.";
    }

    public Boolean IsValid(ValidationContext<T> context, SortOrder value)
        => value.Fields?.All(f => !String.IsNullOrWhiteSpace(f.Name) && (f.Direction == SortDirection.Ascending || f.Direction == SortDirection.Descending)) == true;
}

public class PageValidator<T> : IPropertyValidator<T, PageInfo>
{
    public String Name => "Page";

    public String GetDefaultMessageTemplate(String errorCode)
    {
        return "{PropertyName} is invalid.";
    }

    public Boolean IsValid(ValidationContext<T> context, PageInfo value)
        => value.Number >= 1 && value.Size >= 1;
}
