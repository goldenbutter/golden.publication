using Microchip.Interview.Data;

namespace Microchip.Interview.Tests
{
    public class PublicationXmlFileRepositoryTests
    {
        [Fact]
        public async Task Single_FindsExistingRecord_ById()
        {
            var repo = new XmlPublicationRepository(Environment.PublicationXmlFile);
            var publication = await repo.SingleAsync(Guid.Parse("c3f2b3e4-7d26-4a84-a628-e596f94b1315"));

            Assert.NotNull(publication);
            Assert.Equal("AX3000 Power Regulator", publication.Title);
            Assert.Equal("Datasheet", publication.PublicationType);
            Assert.Equal("978-1-55678-0011", publication.Isbn);
        }

        [Fact]
        public async Task Single_ReturnsNull_WhenIdNotFound()
        {
            var repo = new XmlPublicationRepository(Environment.PublicationXmlFile);

            var publication = await repo.SingleAsync(Guid.NewGuid());

            Assert.Null(publication);
        }

        [Fact]
        public async Task Where()
        {
            var repo = new XmlPublicationRepository(Environment.PublicationXmlFile);

            var actual = await repo.WhereAsync(p =>
                p.Title.Equals("SmartHome Hub Controller", StringComparison.CurrentCultureIgnoreCase));

            Assert.Single(actual);
            Assert.Equal("53c0a927-d475-4c75-b8e3-4b78e26db8b4", actual.Single().Id.ToString());
        }

        [Fact]
        public async Task Where_WithOrderBy()
        {
            var repo = new XmlPublicationRepository(Environment.PublicationXmlFile);
            var actual = await repo.WhereAsync(
                p => p.PublicationType.Equals("User Manual", StringComparison.CurrentCultureIgnoreCase),
                publications => publications.OrderBy(p => p.Title)
            );

            Assert.NotEmpty(actual);
            Assert.Equal("561a7fcb-181d-4d34-9923-942622a6894f", actual.First().Id.ToString());
        }
    }

    public static class Environment
    {
        public static string PublicationXmlFile =
            $"{System.IO.Directory.GetCurrentDirectory()}\\..\\..\\..\\..\\..\\src\\Microchip.Interview.Data\\Data\\publications.xml";
    }
}
