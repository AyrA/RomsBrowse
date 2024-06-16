using Microsoft.EntityFrameworkCore.Migrations;

namespace RomsBrowse.Data
{
    internal static class FulltextAdder
    {
        public static void AddFulltext(MigrationBuilder builder)
        {
            builder.Sql("CREATE FULLTEXT CATALOG RomsFullText AS DEFAULT", true);
            builder.Sql("CREATE FULLTEXT INDEX ON RomFiles(DisplayName) KEY INDEX PK_RomFiles", true);
        }

        public static void RemoveFulltext(MigrationBuilder builder)
        {
            builder.Sql("DROP FULLTEXT INDEX ON RomFiles", true);
            builder.Sql("DROP FULLTEXT CATALOG RomsFullText", true);
        }
    }
}
