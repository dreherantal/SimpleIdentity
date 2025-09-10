using SimpleIdentity.Models;
using SimpleIdentity.Encryption;

namespace SimpleIdentity.DTOs;

public class DTOTools
{
    public static User RegisterDTOtoUser(RegisterDTO registerDTO)
    {

        var user = new User
        {
            Email = registerDTO.Email.ToLower(),
            FirstName = registerDTO.FirstName,
            LastName = registerDTO.LastName,
            PasswordHash = SecretHasher.Hash(registerDTO.Password)
        };

        return user;
    }

}

