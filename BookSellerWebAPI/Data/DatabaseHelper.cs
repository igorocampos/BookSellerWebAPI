using BookSellerWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;

namespace BookSellerWebAPI.Data
{
    public static class DatabaseHelper
    {
        public static IHost CreateDatabase<T>(this IHost webHost) where T : DbContext
        {
            using var scope = webHost.Services.CreateScope();
            var services = scope.ServiceProvider;

            // According to https://github.com/peter-evans/docker-compose-healthcheck/issues/3#issuecomment-329037485
            // docker-compose stopped accepting a health condition in `depends_on` from v3 and above.
            // we are making a resilient application here, trying to reconect 3 times with 5 seconds interval
            Exception lastException = null;
            for (int count = 0; count < 3; count++)
            {
                try
                {
                    var db = services.GetRequiredService<T>();
                    db.Database.Migrate();
                    return webHost;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
                Thread.Sleep(5000);
            }

            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(lastException, "Database Creation/Migrations failed!");

            return webHost;
        }

        public static bool Exists<T>(this DbSet<T> set, long id) where T : BaseModel
           => set.Any(e => e.Id == id);

        //Credit: https://stackoverflow.com/a/33833209
        public static bool IsDefault(this object obj, Type runtimeType)
        {
            if (obj == null)
                return true;

            if (runtimeType == null)
                throw new ArgumentNullException(nameof(runtimeType));

            // Handle non-null reference types.
            if (!runtimeType.IsValueType)
                return false;

            // Nullable, but not null
            if (Nullable.GetUnderlyingType(runtimeType) != null)
                return false;

            // Use CreateInstance as the most reliable way to get default value for a value type
            object defaultValue = Activator.CreateInstance(runtimeType);

            return defaultValue.Equals(obj);
        }
    }
}
