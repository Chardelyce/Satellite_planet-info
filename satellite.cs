/*2023  Charde'Lyce Edwards used with permission under the Gnu Public liscense 
https://www.gnu.org/licenses/gpl-3.0.en.html */

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

//if you see a request failed, it ran out of api requests for the day 
public class SatellitePositionService
{
	private readonly HttpClient _httpClient;
	public SatellitePositionService()
	{
		_httpClient = new HttpClient();
	}

	public async Task<string> GetCelestrakTLEData(string satelliteName)
	{
		string apiUrl = "https://celestrak.org/NORAD/elements/gp.php?GROUP=engineering&FORMAT=tle";
		return await GetDataFromApi(apiUrl, "Celestrak");
	}

	public async Task<string> GetN2YOTLEData(string noradId)
	{
		string apiUrl = $"https://www.n2yo.com/rest/v1/satellite/tle/39084";
		return await GetDataFromApi(apiUrl, "N2YO");
	}

	public async Task<List<string>> GetNASAHorizonsData()
	{
		List<string> planetDataList = new List<string>();
		try
		{
			string[] planetUrls = {"https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='899'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'", "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='799'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'", "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='699'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'", "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='599'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'", "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='499'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'", "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='399'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'", "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='299'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'", "https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND='199'&OBJ_DATA='YES'&MAKE_EPHEM='YES'&EPHEM_TYPE='OBSERVER'&CENTER='500@399'&START_TIME='2006-01-01'&STOP_TIME='2006-01-20'&STEP_SIZE='1%20d'&QUANTITIES='1,9,20,23,24,29'"};
			foreach (var planetUrl in planetUrls)
			{
				var data = await GetDataFromApi(planetUrl, "NASA HORIZONS");
				planetDataList.Add(data);
			}

			return planetDataList;
		}
		catch (Exception ex)
		{
			throw new Exception($"Error retrieving NASA HORIZONS data: {ex.Message}", ex);
		}
	}

	private async Task<string> GetDataFromApi(string apiUrl, string sourceName)
	{
		try
		{
			HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
			if (response.IsSuccessStatusCode)
			{
				string responseData = await response.Content.ReadAsStringAsync();
				if (IsValidResponse(responseData, sourceName))
				{
					return responseData;
				}
				else
				{
					throw new DataValidationException($"Invalid or unexpected data format received from {sourceName}.");
				}
			}
			else
			{
				throw new HttpRequestException($"Request to {sourceName} API failed with status code: {response.StatusCode}");
			}
		}
		catch (HttpRequestException ex)
		{
			throw new Exception($"HTTP request error: {ex.Message}", ex);
		}
		catch (DataValidationException ex)
		{
			throw new Exception($"Data validation error: {ex.Message}", ex);
		}
		catch (Exception ex)
		{
			throw new Exception($"An unexpected error occurred: {ex.Message}", ex);
		}
	}

	private bool IsValidResponse(string responseData, string sourceName)
	{
		switch (sourceName)
		{
			case "Celestrak":
				string[] lines = responseData.Split('\n');
				const int minLines = 3;
				if (lines.Length < minLines)
				{
					Console.WriteLine($"Invalid response from {sourceName}. Expected at least {minLines} lines, but got {lines.Length} lines.");
					Console.WriteLine($"Actual response:\n{responseData}");
					return false;
				}

				for (int i = 0; i < minLines; i++)
				{
					if (string.IsNullOrWhiteSpace(lines[i]))
					{
						Console.WriteLine($"Invalid response from {sourceName}. Line {i + 1} is empty or whitespace.");
						Console.WriteLine($"Actual response:\n{responseData}");
						return false;
					}
				}

				if (!responseData.EndsWith("\n"))
				{
					Console.WriteLine($"Invalid response from {sourceName}. The response does not end with a newline character.");
					Console.WriteLine($"Actual response:\n{responseData}");
					return false;
				}

				return true;
			case "N2YO":
				if (responseData.Contains("TLE_LINE1") && responseData.Contains("TLE_LINE2"))
				{
					return true;
				}
				else
				{
					return false;
				}

			case "NASA HORIZONS":
				if (responseData.Contains("API SOURCE: NASA/JPL Horizons API") && responseData.Contains("Revised:"))
				{
					Console.WriteLine($"Valid response received from {sourceName}. (Planetary Information)");
					return true;
				}
				else
				{
					Console.WriteLine($"Invalid response from {sourceName}. Expected headers are not present.");
					Console.WriteLine($"Actual response:\n{responseData}");
					return false;
				}

			default:
				return true;
		}
	}
}

public class DataValidationException : Exception
{
	public DataValidationException(string message) : base(message)
	{
	}
}

class Program
{
	static async Task Main()
	{
		SatellitePositionService satelliteService = new SatellitePositionService();
		try
		{
			string celestrakData = await satelliteService.GetCelestrakTLEData("Engineering Satellite");
			Console.WriteLine($"Celestrak Data:\n{celestrakData}");
			var nasaDataList = await satelliteService.GetNASAHorizonsData();
			foreach (var nasaData in nasaDataList)
			{
				Console.WriteLine($"NASA Horizons Data:\n{nasaData}");
			}

			string n2yoData = await satelliteService.GetN2YOTLEData("QZEZ47-5KK5WX-VR24YY-558D");
			Console.WriteLine($"N2YO Data:\n{n2yoData}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
		}
	}
}
