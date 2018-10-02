using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CamundaClient.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

namespace signalr_event_hub.Hubs {
    public class AuthenticationHub : Hub {
        private List<User> _users = new List<User>
        { 
            new User { Id = 0, FirstName = "Test", LastName = "User", Username = "serviceaccount", Password = "test" }, 
            new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test" } 
        };

        public void Connected(){
            this.Clients.Caller.SendAsync("connected");
        }

        public async Task<User> Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDFJyYvNjbm4oNpOV02fVwIpg63Afl3FC0GjvrjM3Q1ni7ClOyrxXK5zbgtQFkyVhl/CMgNXjXL013Y5SIPGBsQ5rFVQjAnwZDhgSD5R1eY55QvE3QjF2XuszQedGyUaG7dRRu/00GFYIEF7CcDdOveiiDOvOeMNtb4kztqGO7Xr2oBQNEzWLKEPBq7zucrj9nKOKl1/RlNa+eiOV+9elaytIKJdhr8lKCjzAPPeQfJY7eybUwcoWzlZl8b0nU6fvJKoDNTMk4yEK3SbeFvxOKYOGj2e6FGTKMiXkhfvEDAkJLJwKkdVWSFRjgb/7PA1v259KmV3gk3T1rQ66+29OrRAgMBAAECggEAe6n21Z5YCbMDYrlMsqUnWXVvvXNLm1nYdEizLlhUCF3UTtFDMuuC7vEPGbNHP7+p9nj3owr5C4TlVOtE1dr0/0D08tm1gvpzej+ZA0OwuoRn+q9lJa3Djlpx0riMcvqer8Rth4Fnk9XYmHJsdkqcuNZDheoQA29SoFEZ7478IeUXAFyDRpS0EQlmUlTn5P4fEb8z3vSc7q2aY5wFUiaUKX0ugPK+777gi38h7fIwdNPY01k6jiF/97UcP/ANwt94aR3bu37mhuFeinVUi0tfjHo4LGL6P7exggK4sOQLLd2JIq3TEgMOmcALpx2LBFdXu7QjYd9wbzZwDx+aI0fQAQKBgQDzrQS6cQEGw60la1u85yi7g1Bca0RfyR+yYl6qH4H4rZGIXF9oTgZ9P0B46ZHwbgAekV+JKWZrPt26VBF1GyEo8fF6WJGQ/F1qw2Q5flMR2ueQIPrEFWA/g1TEFFVc6Fp2TS4xBtbWJg4FEbHu4xBycZYzUElqvD+So3nob6GukQKBgQDPH8b9Qcg5AYO6VD2xxAdwSBBlXZG3YQJknuVXGFQlbJRXuYxfnc3uUdb2DduK/sv18uH7mXRccJ8v/ucaKP+0t6AR+VdRFtzAzjKjoTwkPR4zcLkLaELk1/rgtokEKnWwE7UiBl1Fx3ntmK6OU5wr9UUiXMdyLJopV3g/RuoYQQKBgFRafbuI6QENdf/xJUXEg849y/DiVT4PYsCe2wRredO7Shj5WTHDaO2smsYAnTus6K+sRXU29rSDg8A/3/c5GAaTkrN2u5WEN1aBI03f1CPnMqgrMoP0nmf+L7bdDxvld4Nifm4MXwytCcdpc74troDfn05OKcwgNKWvn8D9++txAoGBAMXHXkgvLHXi0Fp4XoEE4uWAqsdgVeh5pcNXRz+nZ5Jk4DH0Z+pV0XKki1NhYCaVr0UnrEqH+ejbUeaOzTbZt3JldWA0bABuiFVDkG9XYwpnohMUrF4MLPRAmLtDEgr8UGzWJLxcv2wGUpNinCwkApinGGD7nyeTF5IqiBRELv0BAoGBAKUAFQLup+WiegQZBgplLje5wtu0fZs1rbDBG3dXEbwI1RHyMTx/Egt271+WwjcBVHKcYmHTOvdUmSzoRZJDrQpEB4EFEoUYYKlJ3+Udu8q5jcRzMs3KhTMfnriOKp6C3yRcfPcanhLfTpcTccHetUZRNxwgs9MK3LJh6pVScm+5");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Upn, user.Username.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
            user.ConnectionId = this.Context.ConnectionId;
            user.Password = null;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Authenticated: {user.Username}");
            Console.WriteLine($"Token: {user.Token}");
            Console.ForegroundColor = ConsoleColor.White;
            return user;
        }
    }
}