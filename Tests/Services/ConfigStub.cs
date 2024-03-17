﻿using MyFlat.Maui.Common;
using MyFlat.Maui.Models;
using System.Security.Cryptography;
using System.Text;

#pragma warning disable CA1416

namespace Tests.Services
{
    public class ConfigStub : IConfig
    {
        int _globusBalanceAccessCount;
        decimal _lastGlobusBalance;

        public static string GlobusUser
        {
            get
            {
                var bytes = new byte[]
                    {
                        1, 0, 0, 0, 208, 140, 157, 223, 1, 21, 209, 17, 140, 122, 0, 192, 79, 194, 151, 235, 1, 0, 0, 0, 144, 125, 37,
                        104, 59, 112, 74, 77, 149, 234, 157, 219, 107, 233, 128, 188, 4, 0, 0, 0, 2, 0, 0, 0, 0, 0, 16, 102, 0, 0, 0,
                        1, 0, 0, 32, 0, 0, 0, 103, 188, 181, 94, 126, 66, 60, 212, 7, 80, 71, 121, 198, 208, 94, 109, 49, 121, 180,
                        53, 23, 139, 4, 236, 73, 226, 15, 206, 131, 29, 208, 215, 0, 0, 0, 0, 14, 128, 0, 0, 0, 2, 0, 0, 32, 0, 0, 0,
                        180, 137, 26, 197, 70, 193, 104, 36, 242, 99, 137, 40, 159, 83, 180, 20, 177, 7, 35, 48, 146, 179, 108, 43,
                        83, 248, 73, 50, 29, 55, 87, 249, 32, 0, 0, 0, 102, 75, 172, 232, 12, 73, 47, 3, 220, 146, 49, 201, 7, 29, 176,
                        235, 165, 147, 249, 91, 183, 78, 118, 220, 167, 144, 58, 22, 234, 57, 21, 46, 64, 0, 0, 0, 67, 170, 93, 9,
                        252, 131, 14, 143, 33, 9, 14, 117, 177, 221, 61, 13, 8, 90, 79, 60, 49, 187, 87, 179, 6, 95, 171, 229, 110, 85,
                        18, 70, 237, 255, 144, 227, 127, 111, 86, 121, 188, 130, 119, 52, 61, 10, 177, 198, 40, 180, 193, 157, 162, 87,
                        128, 172, 242, 60, 53, 193, 1, 222, 64, 39
                    };

                return Encoding.ASCII.GetString(
                    ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine));
            }
        }

        public static string GlobusPassword
        {
            get
            {
                var bytes = new byte[]
                    {
                        1, 0, 0, 0, 208, 140, 157, 223, 1, 21, 209, 17, 140, 122, 0, 192, 79, 194, 151, 235, 1, 0, 0, 0, 144, 125,
                        37, 104, 59, 112, 74, 77, 149, 234, 157, 219, 107, 233, 128, 188, 4, 0, 0, 0, 2, 0, 0, 0, 0, 0, 16, 102, 0,
                        0, 0, 1, 0, 0, 32, 0, 0, 0, 4, 194, 153, 162, 210, 42, 23, 107, 68, 187, 109, 129, 39, 178, 129, 238, 206,
                        219, 49, 228, 33, 89, 209, 167, 34, 41, 120, 112, 11, 148, 225, 59, 0, 0, 0, 0, 14, 128, 0, 0, 0, 2, 0, 0,
                        32, 0, 0, 0, 114, 133, 63, 192, 239, 57, 59, 216, 207, 237, 89, 225, 127, 225, 170, 74, 209, 93, 247, 25,
                        85, 220, 144, 88, 100, 98, 116, 50, 239, 82, 80, 207, 16, 0, 0, 0, 218, 169, 149, 70, 137, 30, 87, 228,
                        204, 107, 231, 27, 173, 144, 61, 125, 64, 0, 0, 0, 206, 144, 67, 30, 128, 14, 199, 244, 150, 122, 243, 100,
                        103, 255, 10, 240, 191, 214, 133, 150, 176, 189, 14, 119, 120, 178, 88, 89, 89, 75, 52, 149, 158, 173, 154,
                        233, 56, 171, 12, 209, 194, 160, 243, 101, 166, 23, 39, 194, 184, 97, 82, 192, 166, 164, 110, 221, 221,
                        216, 141, 8, 140, 82, 78, 234
                    };

                return Encoding.ASCII.GetString(
                    ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine));
            }
        }

        public static string MosOblEircUser
        {
            get
            {
                var bytes = new byte[]
                    {
                        1, 0, 0, 0, 208, 140, 157, 223, 1, 21, 209, 17, 140, 122, 0, 192, 79, 194, 151, 235, 1, 0, 0, 0, 144, 125,
                        37, 104, 59, 112, 74, 77, 149, 234, 157, 219, 107, 233, 128, 188, 4, 0, 0, 0, 2, 0, 0, 0, 0, 0, 16, 102,
                        0, 0, 0, 1, 0, 0, 32, 0, 0, 0, 47, 59, 107, 15, 102, 48, 229, 7, 52, 255, 220, 103, 249, 203, 140, 65, 232,
                        177, 48, 11, 26, 49, 101, 202, 7, 194, 149, 113, 221, 182, 165, 213, 0, 0, 0, 0, 14, 128, 0, 0, 0, 2, 0, 0,
                        32, 0, 0, 0, 197, 175, 132, 71, 215, 183, 122, 87, 127, 2, 169, 83, 127, 181, 83, 250, 20, 162, 131, 61,
                        15, 54, 149, 51, 221, 146, 59, 198, 233, 219, 17, 96, 16, 0, 0, 0, 237, 234, 8, 93, 233, 209, 172, 228,
                        209, 155, 162, 227, 0, 16, 42, 196, 64, 0, 0, 0, 180, 42, 26, 207, 84, 210, 139, 195, 111, 103, 152, 36,
                        111, 167, 238, 55, 43, 137, 121, 71, 30, 94, 52, 90, 194, 28, 169, 253, 232, 65, 156, 81, 183, 88, 181,
                        147, 28, 1, 44, 22, 210, 166, 115, 175, 168, 140, 37, 15, 55, 106, 94, 120, 88, 91, 26, 237, 40, 211,
                        10, 25, 95, 210, 68, 71
                    };

                return Encoding.ASCII.GetString(
                    ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine));
            }
        }

        public static string MosOblEircPassword
        {
            get
            {
                var bytes = new byte[]
                    {
                        1, 0, 0, 0, 208, 140, 157, 223, 1, 21, 209, 17, 140, 122, 0, 192, 79, 194, 151, 235, 1, 0, 0, 0, 144, 125, 37,
                        104, 59, 112, 74, 77, 149, 234, 157, 219, 107, 233, 128, 188, 4, 0, 0, 0, 2, 0, 0, 0, 0, 0, 16, 102, 0, 0, 0,
                        1, 0, 0, 32, 0, 0, 0, 142, 13, 20, 195, 115, 213, 62, 82, 183, 188, 37, 106, 53, 166, 78, 9, 216, 178, 68, 86,
                        119, 198, 100, 36, 6, 28, 102, 99, 113, 45, 169, 32, 0, 0, 0, 0, 14, 128, 0, 0, 0, 2, 0, 0, 32, 0, 0, 0, 233,
                        132, 150, 170, 158, 122, 218, 222, 56, 224, 3, 27, 183, 14, 199, 224, 123, 52, 188, 196, 21, 171, 119, 78, 90,
                        76, 46, 22, 172, 165, 243, 71, 16, 0, 0, 0, 191, 124, 141, 100, 4, 216, 206, 210, 54, 106, 200, 9, 102, 56,
                        200, 44, 64, 0, 0, 0, 67, 46, 222, 12, 75, 133, 129, 242, 152, 231, 142, 179, 167, 183, 175, 119, 233, 165,
                        116, 182, 245, 144, 1, 188, 121, 29, 73, 179, 20, 203, 98, 64, 151, 115, 161, 42, 36, 17, 213, 14, 220, 96,
                        143, 57, 37, 4, 252, 89, 91, 223, 80, 125, 32, 40, 97, 13, 97, 35, 98, 42, 209, 163, 175, 192
                    };

                return Encoding.ASCII.GetString(
                    ProtectedData.Unprotect(bytes, null, DataProtectionScope.LocalMachine));
            }
        }

        public int GetGlobusBalanceAccessCount()
        {
            return _globusBalanceAccessCount;
        }

        public Task<string> GetGlobusPasswordAsync()
        {
            return Task.FromResult(GlobusPassword);
        }

        public Task<string> GetGlobusUserAsync()
        {
            return Task.FromResult(GlobusUser);
        }

        public decimal GetLastGlobusBalance()
        {
            return _lastGlobusBalance;
        }

        public Task<string> GetMosOblEircPasswordAsync()
        {
            return Task.FromResult(MosOblEircPassword);
        }

        public Task<string> GetMosOblEircUserAsync()
        {
            return Task.FromResult(MosOblEircUser);
        }

        public async Task<bool> IsSetAsync()
        {
            return
                !string.IsNullOrWhiteSpace(await GetMosOblEircUserAsync()) &&
                !string.IsNullOrWhiteSpace(await GetMosOblEircPasswordAsync()) &&
                !string.IsNullOrWhiteSpace(await GetGlobusUserAsync()) &&
                !string.IsNullOrWhiteSpace(await GetGlobusPasswordAsync());
        }

        public Task SaveAsync(Settings model)
        {
            return Task.FromResult(true);
        }

        public void SetGlobusBalanceAccessCount(int value)
        {
            _globusBalanceAccessCount = value;
        }

        public Task SetGlobusPasswordAsync(string value)
        {
            return Task.FromResult(true);
        }

        public Task SetGlobusUserAsync(string value)
        {
            return Task.FromResult(true);
        }

        public void SetLastGlobusBalance(decimal value)
        {
            _lastGlobusBalance = value;
        }

        public Task SetMosOblEircPasswordAsync(string value)
        {
            return Task.FromResult(true);
        }

        public Task SetMosOblEircUserAsync(string value)
        {
            return Task.FromResult(true);
        }
    }
}
