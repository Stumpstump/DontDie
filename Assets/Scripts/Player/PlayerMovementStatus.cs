using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Player
{
    [Serializable]
    public enum PlayerMovementStatus
    {
        Default, Idling, Walking, BleedingOut, Jumping, Falling, Sliding
    }
}
