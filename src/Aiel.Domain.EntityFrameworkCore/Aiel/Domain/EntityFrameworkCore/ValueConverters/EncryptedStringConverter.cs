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

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Cryptography;
using System.Text;

namespace Aiel.Domain.EntityFrameworkCore.ValueConverters;

/// <summary>
/// A value converter for encrypting and decrypting string values in Entity Framework Core.
/// </summary>
public class EncryptedStringConverter : ValueConverter<String, String>
{
    // In production, load this from secure configuration (Azure Key Vault, etc.)
    private static readonly Byte[] Key;

    static EncryptedStringConverter()
    {
        // Initialize encryption key from environment or config
        // IMPORTANT: Never hardcode keys in production code
        var keyString = Environment.GetEnvironmentVariable("ENCRYPTION_KEY")
            ?? throw new InvalidOperationException("ENCRYPTION_KEY not configured");

        // Derive a consistent key from the string
        Key = SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EncryptedStringConverter"/> class.
    /// </summary>
    public EncryptedStringConverter() : base(
        v => Encrypt(v),
        v => Decrypt(v))
    {
    }

    private static String Decrypt(String cipherText)
    {
        if (String.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        using var aes = Aes.Create();
        aes.Key = Key;

        var cipherBytesWithIv = Convert.FromBase64String(cipherText);
        var iv = cipherBytesWithIv.Take(aes.BlockSize / 8).ToArray();
        var cipherBytes = cipherBytesWithIv.Skip(aes.BlockSize / 8).ToArray();

        aes.IV = iv;
        using var decryptor = aes.CreateDecryptor();
        var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes);
    }

    private static String Encrypt(String plainText)
    {
        if (String.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Store the IV with the ciphertext; the IV is required for decryption
        var result = new Byte[aes.IV.Length + encryptedBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

        return Convert.ToBase64String(result);
    }
}
