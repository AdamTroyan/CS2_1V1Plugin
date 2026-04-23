using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _1v1Round
{
    public class PlayerWeaponState
    {
        public List<string> Weapons { get; set; } = new List<string>();
        public string ActiveWeapon { get; set; } = "";
    }
}
