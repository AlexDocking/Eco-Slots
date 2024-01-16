using Eco.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parts
{
    public readonly struct VersionNumber
    {
        public string ModName { get; init; }
        public int Major { get; init; }
        public int Minor { get; init; }
        public int Revision { get; init; }
    }
    public static class PartsModVersionNumber
    {
        public static readonly VersionNumber Current = new VersionNumber() { ModName = "Parts", Major = 1, Minor = 0, Revision = 0 };
    }
    public interface IMigratable
    {
        public VersionNumber Version { get; internal set; }
    }
    public static class MigrationManager
    {
        public static T Migrate<T>(object context, object data) where T : class
        {
            if (data is IMigratable intermediateMigratedData)
            {
                Queue<IModMigrator> migrationQueue = new Queue<IModMigrator>();
                while (migrationQueue.TryDequeue(out IModMigrator migrator))
                {
                    intermediateMigratedData = migrator.Migrate(context, intermediateMigratedData);
                    intermediateMigratedData.Version = migrator.Version;
                }
                return intermediateMigratedData as T;
            }
            return data as T;
        }
    }
    public class ModMigrationAttribute : Attribute
    {

    }
    public interface IModMigrator
    {
        public IMigratable Migrate(object context, object data);
        public VersionNumber Version { get; }
    }
}