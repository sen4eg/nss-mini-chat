using System.ComponentModel.DataAnnotations;

namespace MiniServer.Data.Model; 

public class AuthenicatedToken {
    

    [Key]
    public string Token { get; set; }
    public string Device { get; set; }
    public string Username { get; set; }
    public string ip { get; set; }
    public User User { get; set; }
    
    public string OS;

}