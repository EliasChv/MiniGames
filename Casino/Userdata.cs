using Godot;
using System.Text.Json;

public partial class Userdata : Node {
    public class Data {
        public int Balance { get; set; }
    }
    public Data data = new() {
        Balance = 200
    };
    public Userdata readData () {
        if (!FileAccess.FileExists("user://data.save"))
        {
            var file = FileAccess.Open("user://data.save", FileAccess.ModeFlags.Write);

            file.StoreString(JsonSerializer.Serialize(data));

            file.Close();
        }

        var openFile = FileAccess.Open("user://data.save", FileAccess.ModeFlags.Read);
        string json = openFile.GetAsText();

        data = JsonSerializer.Deserialize<Data>(json);

        return this;
    }
    public Userdata writeData () {
        var file = FileAccess.Open("user://data.save", FileAccess.ModeFlags.Write);

        file.StoreString(JsonSerializer.Serialize(data));

        file.Close();

        return this;
    }
}