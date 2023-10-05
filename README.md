Package provides easy access to stored data through data repositories.
<br>
<br>
<b>Usage:</b>
```csharp
public class User
{
  public int Id;
  public string Name;
}

public class UserDataAccess : IDataAccess<int, User>
{
  Task SaveAsync(int key, User data) => throw new NotImplementedException();
  Task<User> LoadAsync(int key) => throw new NotImplementedException();
  Task<User> CreateNewAsync(int key) => throw new NotImplementedException();
  Task DeleteAsync(int key) => throw new NotImplementedException();
}

public static void Main()
{
  var repository = new EntityRepository<int, User>(new UserDataAccess());
  Repository.Register(repository);
  
  // Wherenever you need to access the repository and data
  var repository = Repository.Get<int, User>()!;
  using (var user = repository.GetEntityWrite(1))
  {
    user.Data.Name = "John Doe";
    user.Save();
  }
  
  using (var user = repository.GetEntityRead(1))
  {
    Console.WriteLine(user.Data.Name);
  }
}
```