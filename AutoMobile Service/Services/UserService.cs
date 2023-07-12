using AutoMobile_Service.Data;
using System.Text.Json;

namespace AutoMobile_Service.Services;
public static class UserService
{
    public const string DefaultUserName = "admin";
    public const string DefaultPassword = "admin";

    private static void SaveUser(List<UserModel> users)
    {
        string directoryPath = Utils.GetDirectoryPath();
        string userFilePath = Utils.GetUsersFilePath();

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var serializedUserJson = JsonSerializer.Serialize(users);
        File.WriteAllText(userFilePath, serializedUserJson);
    }

    public static List<UserModel> FetchUsers()
    {
        string userFilePath = Utils.GetUsersFilePath();

        if (!File.Exists(userFilePath))
        {
            return new List<UserModel>();
        }

        var userData = File.ReadAllText(userFilePath);
        return JsonSerializer.Deserialize<List<UserModel>>(userData);
    }

    public static List<UserModel> RegisterUser(Guid Id, string userName, string firstName, string lastName, string password, UserType type)
    {
        List<UserModel> users = FetchUsers();
        bool userNameExistence = users.Any(user => user.UserName == userName);

        if (userNameExistence)
        {
            throw new Exception("Username is already in use!");
        }

        if (type == UserType.Admin)
        {
            int totalAdmin = users.Count(user => user.UType == UserType.Admin);

            if (totalAdmin > 1)
            {
                throw new Exception("There are already two admins!");
            }
        }

        users.Add(
            new UserModel
            {
                UserName = userName,
                Password = Utils.HashPassword(password),
                UType = type,
                RegisterdBy = Id,
                FirstName = firstName,
                LastName = lastName,
            }
        );

        SaveUser(users);
        return users;
    }

    public static UserModel FetchUserById(Guid Id)
    {
        List<UserModel> users = FetchUsers();

        return users.FirstOrDefault(user => user.Id == Id);
    }

    public static List<UserModel> DeleteUser(Guid Id)
    {
        List<UserModel> users = FetchUsers();
        UserModel findUser = users.FirstOrDefault(user => user.Id == Id);

        if (findUser == null)
        {
            throw new Exception("Selected user not found");
        }

        List<ItemHistory> records = ItemService.FetchItemHistory();
        records.RemoveAll(record => findUser.Id == record.TakenBy || findUser.Id == record.ActionedBy);

        ItemService.SaveItemHistory(records);
        users.Remove(findUser);
        SaveUser(users);

        return users;
    }

    public static UserModel Login(string username, string password)
    {
        var errorMessage = "Invalid Credentials";

        List<UserModel> users = FetchUsers();
        UserModel user = users.FirstOrDefault(user => user.UserName == username);

        if (user == null)
        {
            throw new Exception(errorMessage);
        }

        bool checkPassword = Utils.HashVerification(password, user.Password);

        if (!checkPassword)
        {
            throw new Exception(errorMessage);
        }

        return user;
    }

    public static UserModel ChangeUserPassword(Guid id, string currentPassword, string newPassword)
    {
        List<UserModel> users = FetchUsers();
        UserModel user = users.FirstOrDefault(user => user.Id == id);

        if (user == null)
        {
            throw new Exception("User Not Found!");
        }

        bool verifyOldPassword = Utils.HashVerification(currentPassword, user.Password);

        if (!verifyOldPassword)
        {
            throw new Exception("Invalid Current Credential!");
        }

        if (currentPassword == newPassword)
        {
            throw new Exception("This password is already in use!");
        }

        if (newPassword.Length < 1)
        {
            throw new Exception("Password cannot be empty!");
        }

        user.Password = Utils.HashPassword(newPassword);
        user.HasInitialPassword = false;

        SaveUser(users);
        return user;
    }

    public static UserModel ChangeUserDetail(Guid id, string firstName, string lastname, string password)
    {
        List<UserModel> users = FetchUsers();
        UserModel user = users.FirstOrDefault(user => user.Id == id);

        if (user == null)
        {
            throw new Exception("User Not Found!");
        }

        bool verifyOldPassword = Utils.HashVerification(password, user.Password);

        if (!verifyOldPassword)
        {
            throw new Exception("Invalid Current Credential!");
        }

        if (firstName.Length < 1 || lastname.Length < 1)
        {
            throw new Exception("Name cannot be empty!");
        }

        user.FirstName = firstName;
        user.LastName = lastname;

        SaveUser(users);
        return user;
    }

    public static void SeedUserData()
    {
        var users = FetchUsers().FirstOrDefault(user => user.UType == UserType.Admin);

        if (users == null)
        {
            RegisterUser(Guid.Empty, DefaultUserName, "Admin", "Admin", DefaultPassword, UserType.Admin);
        }
    }
}
