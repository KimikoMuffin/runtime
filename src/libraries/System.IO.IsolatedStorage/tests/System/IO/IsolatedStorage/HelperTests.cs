﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using Xunit;

namespace System.IO.IsolatedStorage.Tests
{
    public class HelperTests
    {
        [Theory
            InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 }, @"aaaaaaaa")
            InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, @"55555555")
            InlineData(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, @"aaaaaaaaaaaaaaaa")
            InlineData(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }, @"abcdeaaafghijaaa")
            InlineData(
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                @"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")
            ]
        public void ToBase32StringSuitableForDirName(byte[] buff, string expected)
        {
            // Validating that the legacy Path.ToBase32StringSuitableForDirName results match
            // our copy of the code. Results should NOT change as IsolatedStorage depends
            // on this for creating a stable hash based directory name.
            Assert.Equal(expected, Helper.ToBase32StringSuitableForDirName(buff));
        }

        [Fact]
        public void GetNormalizedStrongNameHash()
        {
            // Validating that we match the exact hash the desktop IsolatedStorage implementation would create.
            Assert.Equal(@"10nbq10da2m1qfsisndjihnhqmilalwl", Helper.GetNormalizedStrongNameHash(GetAssemblyNameWithFullKey()));
        }

        [Fact]
        public void GetNormalizedUrlHash()
        {
            // Validating that we match the exact hash the desktop IsolatedStorage implementation would create.
            Uri uri = new Uri(@"file://C:/Users/jerem/Documents/Visual Studio 2015/Projects/LongPath/LongPath/bin/Debug/TestAssembly.EXE");
            Assert.Equal(@"qgeirsoc3cznuklvq5xlalurh1m0unxl", Helper.GetNormalizedUriHash(uri));
        }

        private static AssemblyName GetAssemblyNameWithFullKey()
        {
            byte[] publicKey = new byte[]
            {
                0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00,
                0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x37, 0x52, 0x6e, 0xdf, 0xc0, 0x62, 0x17, 0x9f,
                0x9d, 0x24, 0xe5, 0x0d, 0x3f, 0x9b, 0xb1, 0x34, 0xe1, 0x7e, 0x14, 0x9e, 0xb5, 0x19, 0xcc, 0x2f, 0xd1, 0x9c, 0x39, 0x08,
                0x46, 0xf4, 0x18, 0xba, 0x6b, 0x2b, 0xe1, 0xc2, 0xb7, 0xe9, 0x06, 0x59, 0x57, 0xed, 0xe1, 0x83, 0x9c, 0xc8, 0x66, 0x4f,
                0xba, 0x3a, 0x05, 0x6b, 0x73, 0x98, 0x56, 0x0a, 0x34, 0x8e, 0x69, 0xf1, 0x4a, 0x69, 0x4f, 0x4f, 0xea, 0xc7, 0x3e, 0x27,
                0xf6, 0x6f, 0xd5, 0x4c, 0xcb, 0xeb, 0xe3, 0xa7, 0x5f, 0x3c, 0x11, 0xd3, 0x82, 0xc7, 0xee, 0x1a, 0x5c, 0xf6, 0x37, 0x8c,
                0xc9, 0x81, 0xbb, 0xb8, 0xa4, 0xab, 0xe6, 0x9d, 0x10, 0x96, 0x3a, 0xf8, 0xa0, 0xaa, 0x42, 0xb4, 0x45, 0xb1, 0x6c, 0xe3,
                0x9b, 0xc5, 0xb0, 0x84, 0x29, 0x32, 0x20, 0xc8, 0xb9, 0x5b, 0x1d, 0x40, 0xec, 0xbe, 0x23, 0x2e, 0x6b, 0xdd, 0x5d, 0xc4
            };

            AssemblyName name = new AssemblyName();
            name.Name = "TestAssembly";
            name.Version = new Version(1, 0);
            name.SetPublicKey(publicKey);

            return name;

            // C:\Users\jerem\AppData\Local\IsolatedStorage\10v31ho4.bo2\eeolfu22.f2w\Url.qgeirsoc3cznuklvq5xlalurh1m0unxl\AssemFiles\
            // C:\Users\jerem\AppData\Local\IsolatedStorage\10v31ho4.bo2\eeolfu22.f2w\StrongName.10nbq10da2m1qfsisndjihnhqmilalwl\AssemFiles\
        }

        [Fact]
        public void GetDefaultIdentityAndHash()
        {
            object identity;
            string hash;
            Helper.GetDefaultIdentityAndHash(out identity, out hash, '.');

            Assert.NotNull(identity);
            Assert.NotNull(hash);

            // We lie about the identity type when creating the folder structure as we're emulating the Evidence types
            // we don't have available in .NET Standard. We don't serialize the actual identity object, so the desktop
            // implementation will work with locations built off the hash.
            if (identity.GetType() == typeof(Uri))
            {
                Assert.StartsWith(@"Url.", hash);
            }
            else
            {
                Assert.IsType<AssemblyName>(identity);
                Assert.StartsWith(@"StrongName.", hash);
            }
        }

        [Theory
            InlineData(IsolatedStorageScope.Assembly)
            InlineData(IsolatedStorageScope.Assembly | IsolatedStorageScope.Roaming)
            // Need to implement ACLing to test this one
            // InlineData(IsolatedStorageScope.Machine)
            ]
        public void GetDataDirectory(IsolatedStorageScope scope)
        {
            string path = Helper.GetDataDirectory(scope);
            Assert.Equal("IsolatedStorage", Path.GetFileName(path));
        }

        [Fact]
        public void GetExistingRandomDirectory()
        {
            using (var temp = new TempDirectory())
            {
                Assert.Null(Helper.GetExistingRandomDirectory(temp.Path));

                string randomPath = Path.Combine(temp.Path, Path.GetRandomFileName(), Path.GetRandomFileName());
                Directory.CreateDirectory(randomPath);
                Assert.Equal(randomPath, Helper.GetExistingRandomDirectory(temp.Path));
            }
        }

        [Theory
            InlineData(IsolatedStorageScope.User)
            // Need to implement ACLing
            // InlineData(IsolatedStorageScope.Machine)
            ]
        public void GetRandomDirectory(IsolatedStorageScope scope)
        {
            using (var temp = new TempDirectory())
            {
                string randomDir = Helper.GetRandomDirectory(temp.Path, scope);
                Assert.True(Directory.Exists(randomDir));
            }
        }
    }
}
