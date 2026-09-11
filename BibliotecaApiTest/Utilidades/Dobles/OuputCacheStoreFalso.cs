using Microsoft.AspNetCore.OutputCaching;
using System;
using System.Collections.Generic;
using System.Text;

namespace BibliotecaApiTest.Utilidades.Dobles
{
    /// <summary>
    /// clase falsa para cache
    /// </summary>
    internal class OuputCacheStoreFalso : IOutputCacheStore
    {
        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;

        }

        public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
