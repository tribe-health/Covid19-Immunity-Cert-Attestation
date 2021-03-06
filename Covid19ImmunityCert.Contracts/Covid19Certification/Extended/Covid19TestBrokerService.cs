using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using Nethereum.RPC.Eth.DTOs;

using Covid19ImmunityCert.Contracts.Covid19Certification.ContractDefinition;

namespace Covid19ImmunityCert.Contracts.Covid19Certification
{
	public partial class Covid19TestBrokerService
	{
		public const double COORDINATE_RESOLUTION = 1000000000000000.0;

		public async Task<SampleCentre> GetClosestSampleCentreWithAvailableTests(GetSampleCentresWithAvailableTestsFunction sampleCentresFunction, GeoCoordinate currLocation, BlockParameter blockParameter = null)
		{
			GetSampleCentresWithAvailableTestsOutputDTO availableTestsOutput =
				await SampleCentresWithAvailableTestsQueryAsync(sampleCentresFunction, blockParameter).ConfigureAwait(false);

			var availableTestCentres = availableTestsOutput.ReturnValue1;

			var nearestLoc
				= availableTestCentres.Select(x => new GeoCoordinate(ToDbl(x.Location.Lat), ToDbl(x.Location.Long)))
					                  .OrderBy(x => x.GetDistanceTo(currLocation))
					                  .First();

			return FindSampleCentreByLocation(availableTestCentres, nearestLoc);
		}

		public async Task<SampleCentre> GetClosestSampleCentreWithAvailableTests(GetSampleCentresWithAvailableTestsFunction sampleCentresFunction,
			                                                                                                  GeoCoordinate currLocation,
																											           uint acceptedHotZoneLevel,
																											 BlockParameter blockParameter = null)
		{
			GetSampleCentresWithAvailableTestsOutputDTO availableTestsOutput =
				await SampleCentresWithAvailableTestsQueryAsync(sampleCentresFunction, blockParameter).ConfigureAwait(false);

			var availableTestCentres = availableTestsOutput.ReturnValue1;

			var nearestLoc
				= availableTestCentres.Where(x => x.HotZoneLevel <= acceptedHotZoneLevel)
									  .Select(x => new GeoCoordinate(ToDbl(x.Location.Lat), ToDbl(x.Location.Long)))
									  .OrderBy(x => x.GetDistanceTo(currLocation))
					                  .First();

			return FindSampleCentreByLocation(availableTestCentres, nearestLoc);
		}

		public async Task<SampleCentre> GetClosestSampleCentreWithAvailableTests(GetSampleCentresWithAvailableTestsFunction sampleCentresFunction,
																											  GeoCoordinate currLocation,
																													   uint acceptedHotZoneLevel,
																													   uint minAvgTestSensitivity,
																													   uint minAvgTestSpecificity,
																											 BlockParameter blockParameter = null)
		{
			GetSampleCentresWithAvailableTestsOutputDTO availableTestsOutput =
				await SampleCentresWithAvailableTestsQueryAsync(sampleCentresFunction, blockParameter).ConfigureAwait(false);

			var availableTestCentres = availableTestsOutput.ReturnValue1;

			var nearestLoc
				= availableTestCentres.Where(x => x.HotZoneLevel <= acceptedHotZoneLevel)
				                      .Where(x => x.AvgTestSensitivtyRating >= minAvgTestSensitivity)
									  .Where(x => x.AvgTestSpecifityRating >= minAvgTestSpecificity)
									  .Select(x => new GeoCoordinate(ToDbl(x.Location.Lat), ToDbl(x.Location.Long)))
									  .OrderBy(x => x.GetDistanceTo(currLocation))
									  .First();

			return FindSampleCentreByLocation(availableTestCentres, nearestLoc);
		}

		public static SampleCentre FindSampleCentreByLocation(List<SampleCentre> sampleCentres, GeoCoordinate location)
		{
			var foundCentre =
				sampleCentres
				.Where(x => (ToDbl(x.Location.Lat) == location.Latitude) && (ToDbl(x.Location.Long) == location.Longitude))
				.First();

			if (foundCentre == null)
				foundCentre = new SampleCentre();

			return foundCentre;
		}

		public static double ToDbl(BigInteger coordinateValue)
		{
			return ((double)coordinateValue) / COORDINATE_RESOLUTION;
		}
	}
}