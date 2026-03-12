namespace Ninjadog.Tests.E2E.Fixtures;

[CollectionDefinition("NinjadogUi")]
public class NinjadogUiCollection : ICollectionFixture<NinjadogUiFixture>, ICollectionFixture<PlaywrightFixture>;
