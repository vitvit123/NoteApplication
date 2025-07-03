using System;
using System.Collections.Concurrent;

namespace NoteAppApi.Security
{
    public static class TokenBlacklist
    {
        private static readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

        public static void Add(string jti, DateTime expiry)
        {
            _blacklist.TryAdd(jti, expiry);
        }

        public static bool IsBlacklisted(string jti)
        {
            if (_blacklist.TryGetValue(jti, out var expiry))
            {
                if (expiry > DateTime.UtcNow)
                    return true;

                _blacklist.TryRemove(jti, out _);
            }
            return false;
        }
    }
}
