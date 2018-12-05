using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace MockFramework
{
	public class ThingCache
	{
		private readonly IDictionary<string, Thing> dictionary
			= new Dictionary<string, Thing>();
		private readonly IThingService thingService;

		public ThingCache(IThingService thingService)
		{
			this.thingService = thingService;
		}

		public Thing Get(string thingId)
		{
			Thing thing;
			if (dictionary.TryGetValue(thingId, out thing))
				return thing;
			if (thingService.TryRead(thingId, out thing))
			{
				dictionary[thingId] = thing;
				return thing;
			}
			return null;
		}
	}

	[TestFixture]
	public class ThingCache_Should
	{
		private IThingService thingService;
		private ThingCache thingCache;

		private const string thingId1 = "TheDress";
		private Thing thing1 = new Thing(thingId1);

		private const string thingId2 = "CoolBoots";
		private Thing thing2 = new Thing(thingId2);

		private const string nullThingId = "nullThing";
		private Thing nullThing = new Thing(nullThingId);
		
		[SetUp]
		public void SetUp()
		{
			thingService = A.Fake<IThingService>();

			A.CallTo(() => thingService.TryRead(thingId1, out thing1)).Returns(true);
			A.CallTo(() => thingService.TryRead(thingId2, out thing2)).Returns(true);
			A.CallTo(() => thingService.TryRead(nullThingId, out nullThing)).Returns(false);
			
			thingCache = new ThingCache(thingService);
		}

		[Test]
		public void ReturnInstanceFromService()
		{
			var thing = thingCache.Get(thingId1);
			
			thing.ShouldBeEquivalentTo(thing1);
		}
		
		[Test]
		public void ReturnDifferentInstanceFromService_OnDifferentIds()
		{
			var actualThing1 = thingCache.Get(thingId1);
			var actualThing2 = thingCache.Get(thingId2);
			
			actualThing1.ShouldBeEquivalentTo(thing1);
			actualThing2.ShouldBeEquivalentTo(thing2);
		}
		
		[Test]
		public void ReturnSameInstanceFromService_OnSameIds()
		{
			var actualThing1 = thingCache.Get(thingId1);
			var actualThing2 = thingCache.Get(thingId1);
			
			actualThing1.ShouldBeEquivalentTo(actualThing2);
		}

		[Test]
		public void CallsServiceOnce_OnSameThingId()
		{
			thingCache.Get(thingId1);
			thingCache.Get(thingId1);
			
			A.CallTo(() => thingService.TryRead(thingId1, out thing1)).MustHaveHappened(Repeated.Exactly.Once);
		}
		
		[Test]
		public void CallsServiceTwice_OnTwoDifferentThingIds()
		{
			Thing _;
			
			thingCache.Get(thingId1);
			thingCache.Get(thingId2);
			
			A.CallTo(() => thingService.TryRead(A<string>.Ignored, out _)).MustHaveHappened(Repeated.Exactly.Twice);
		}

		[Test]
		public void ReturnsNull_OnThingNotInService()
		{
			var nullThingActual = thingCache.Get(nullThingId);
			
			nullThingActual.Should().BeNull();
		}

		[Test]
		public void CallsServiceOnceForEachThing_OnTwoDifferentThingIds()
		{
			Thing _;
			
			thingCache.Get(thingId1);
			thingCache.Get(thingId2);
			
			A.CallTo(() => thingService.TryRead(thingId1, out _)).MustHaveHappened(Repeated.Exactly.Once);
			A.CallTo(() => thingService.TryRead(thingId2, out _)).MustHaveHappened(Repeated.Exactly.Once);
		}
	}
}