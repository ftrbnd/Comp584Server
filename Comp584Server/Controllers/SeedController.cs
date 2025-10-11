using Comp584Server.Data;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using WorldModel;

namespace Comp584Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(Comp584Context context, IHostEnvironment hostEnvironment) : ControllerBase
    {
        string _pathName = Path.Combine(hostEnvironment.ContentRootPath, "Data/worldcities.csv");

        [HttpPost("Countries")]
        public async Task<ActionResult> PostCountries()
        {
            Dictionary<string, Country> countries = await context.Countries
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);

            CsvConfiguration config = new(CultureInfo.InvariantCulture) { 
                HasHeaderRecord = true, 
                HeaderValidated = null 
            }; 
            using StreamReader reader = new(_pathName);
            using CsvReader csv = new(reader, config);
            List<Comp584Csv> records = csv.GetRecords<Comp584Csv>().ToList();

            foreach (Comp584Csv record in records) {
                if (!countries.ContainsKey(record.country)) {
                    Country country = new()
                    {
                        Name = record.country,
                        Iso2 = record.iso2,
                        Iso3 = record.iso3
                    };
                    countries.Add(country.Name, country);
                    await context.Countries.AddAsync(country);
                }
            }

            await context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Cities")]
        public async Task<ActionResult> PostCities()
        {
            Dictionary<string, Country> countries = await context.Countries
               .AsNoTracking()
               .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };
            using StreamReader reader = new(_pathName);
            using CsvReader csv = new(reader, config);
            List<Comp584Csv> records = csv.GetRecords<Comp584Csv>().ToList();
            
            int cityCount = 0;
            foreach (Comp584Csv record in records)
            {
                if (record.population.HasValue && record.population.Value > 0)
                {
                    City city = new()
                    {
                        Name = record.city,
                        Lat = (double)record.lat,
                        Lng = (double)record.lng,
                        Population = (long)record.population.Value,
                        CountryId = countries[record.country].Id
                    };
                    await context.Cities.AddAsync(city);
                    cityCount++;
                }
            }

            await context.SaveChangesAsync();

            return new JsonResult(cityCount);
        }
    }
}
