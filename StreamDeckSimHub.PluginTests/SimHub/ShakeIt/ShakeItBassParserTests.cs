using StreamDeckSimHub.Plugin.SimHub.ShakeIt;

namespace StreamDeckSimHub.PluginTests.SimHub.ShakeIt;

public class ShakeItBassParserTests
{
    private ShakeItBassParser _parser = null!;

    [SetUp]
    public void Init()
    {
        _parser = new ShakeItBassParser();
    }

    [Test]
    public void ParseStructure()
    {
        var data = @"
0: id0 My first Profile
  1: id0-0 GroupContainer Container 0
    2: id0-0-0 EffectsContainerBase RPMS (for V8)
    2: id0-0-1 GroupContainer Container 0-0
      3: id0-0-1-0 EffectsContainerBase Wheels Lock
    2: id0-0-2 GroupContainer Container 0-1
      3: id0-0-2-0 GroupContainer Container 0-1-0
        4: id0-0-2-0-0 EffectsContainerBase Speed
    2: id0-0-3 EffectsContainerBase RPMS (for V6)
0: id1 My second Profile
  1: id1-0 EffectsContainerBase Gear Shift";

        var reader = new StringReader(data);
        for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
        {
            _parser.ParseLine(line);
        }

        var profiles = _parser.Profiles;
        Assert.That(profiles.Count, Is.EqualTo(2));
        Assert.That(profiles[0].Id, Is.EqualTo("id0"));
        Assert.That(profiles[0].Name, Is.EqualTo("My first Profile"));
        Assert.That(profiles[0].EffectsContainers.Count, Is.EqualTo(1));

        var container0 = profiles[0].EffectsContainers[0] as GroupContainer;
        Assert.That(container0, Is.Not.Null);
        Assert.That(container0.Id, Is.EqualTo("id0-0"));
        Assert.That(container0.Name, Is.EqualTo("Container 0"));
        Assert.That(container0.EffectsContainers.Count, Is.EqualTo(4));

        Assert.That(container0.EffectsContainers[0].Id, Is.EqualTo("id0-0-0"));
        Assert.That(container0.EffectsContainers[0].Name, Is.EqualTo("RPMS (for V8)"));
        Assert.That(container0.EffectsContainers[0] is GroupContainer, Is.False);

        var container01 = container0.EffectsContainers[1] as GroupContainer;
        Assert.That(container01, Is.Not.Null);
        Assert.That(container01.Id, Is.EqualTo("id0-0-1"));
        Assert.That(container01.Name, Is.EqualTo("Container 0-0"));
        Assert.That(container01.EffectsContainers.Count, Is.EqualTo(1));

        Assert.That(container01.EffectsContainers[0].Id, Is.EqualTo("id0-0-1-0"));
        Assert.That(container01.EffectsContainers[0].Name, Is.EqualTo("Wheels Lock"));
        Assert.That(container01.EffectsContainers[0] is GroupContainer, Is.False);

        var container02 = container0.EffectsContainers[2] as GroupContainer;
        Assert.That(container02, Is.Not.Null);
        Assert.That(container02.Id, Is.EqualTo("id0-0-2"));
        Assert.That(container02.Name, Is.EqualTo("Container 0-1"));
        Assert.That(container02.EffectsContainers.Count, Is.EqualTo(1));

        var container03 = container02.EffectsContainers[0] as GroupContainer;
        Assert.That(container03, Is.Not.Null);
        Assert.That(container03.Id, Is.EqualTo("id0-0-2-0"));
        Assert.That(container03.Name, Is.EqualTo("Container 0-1-0"));
        Assert.That(container03.EffectsContainers.Count, Is.EqualTo(1));

        Assert.That(container03.EffectsContainers[0].Id, Is.EqualTo("id0-0-2-0-0"));
        Assert.That(container03.EffectsContainers[0].Name, Is.EqualTo("Speed"));
        Assert.That(container03.EffectsContainers[0] is GroupContainer, Is.False);

        Assert.That(container0.EffectsContainers[3].Id, Is.EqualTo("id0-0-3"));
        Assert.That(container0.EffectsContainers[3].Name, Is.EqualTo("RPMS (for V6)"));
        Assert.That(container0.EffectsContainers[3] is GroupContainer, Is.False);

        Assert.That(profiles[1].Id, Is.EqualTo("id1"));
        Assert.That(profiles[1].Name, Is.EqualTo("My second Profile"));
        Assert.That(profiles[1].EffectsContainers.Count, Is.EqualTo(1));

        Assert.That(profiles[1].EffectsContainers[0].Id, Is.EqualTo("id1-0"));
        Assert.That(profiles[1].EffectsContainers[0].Name, Is.EqualTo("Gear Shift"));
        Assert.That(profiles[1].EffectsContainers[0] is GroupContainer, Is.False);
    }
}