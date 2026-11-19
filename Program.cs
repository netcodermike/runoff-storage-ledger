using System.Text.Json;

try
{
    if (args.Length != 1) throw new ArgumentException("Pass one JSON input file");
    using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
    Console.WriteLine(JsonSerializer.Serialize(Analyze(data.RootElement)));
    return 0;
}
catch (Exception error) when (error is IOException or JsonException or FormatException
                               or ArgumentException or InvalidOperationException
                               or KeyNotFoundException or OverflowException)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}

static object Analyze(JsonElement input) {
 decimal Read(JsonElement row,string key) {var value=row.GetProperty(key).GetDecimal();if(value<0)throw new ArgumentException("Values must be nonnegative");return value;}
 var capacity=Read(input,"capacityLiters");var stored=Read(input,"initialLiters");var area=Read(input,"roofM2");var fraction=Read(input,"captureFraction");
 if(capacity<=0||stored>capacity||fraction>1)throw new ArgumentException("Invalid capacity, fill, or capture fraction");
 decimal overflow=0,unmet=0;var ledger=new List<object>();
 foreach(var day in input.GetProperty("days").EnumerateArray()) {var capture=Read(day,"rainMm")*area*fraction;var demand=Read(day,"demandLiters");var spill=Math.Max(0,stored+capture-capacity);stored=Math.Min(capacity,stored+capture);var served=Math.Min(stored,demand);stored-=served;overflow+=spill;unmet+=demand-served;ledger.Add(new{captureLiters=capture,spillLiters=spill,servedLiters=served,storedLiters=stored});}
 return new{finalLiters=stored,overflowLiters=overflow,unmetLiters=unmet,ledger};
}
