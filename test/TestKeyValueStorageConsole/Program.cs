using Ddon.KeyValueStorage;

Console.WriteLine("Hello, World!");

var valuePairs = DdonKvStorageFactory<Student>.GetInstance(slice: "abc");

Student student = new()
{
    Age = 10,
    Name = "这\t是\t测\t试"
};

await valuePairs.SetValueAsync(Guid.NewGuid().ToString(), student);
await valuePairs.SaveAsync();

Console.WriteLine("1保存完成");

var values = await valuePairs.GetAllValueAsync();
foreach (var value in values)
{
    Console.WriteLine(value!.Name);
}

var valuePairs2 = DdonKvStorageFactory<Student>.GetInstance();

var count = (await valuePairs2.GetAllKeyAsync()).Count();
await valuePairs2.SetValueAsync(count + 1, student);

Console.WriteLine("2保存完成");

class Student
{
    public int Age { get; set; }
    public string? Name { get; set; }
}