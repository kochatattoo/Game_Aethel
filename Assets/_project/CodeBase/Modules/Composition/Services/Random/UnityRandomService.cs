using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBase.Infrastructure.Services
{
    public class UnityRandomService : IRandomService
    {
        public int Next(int min, int max)
        {
            return new Random().Next(min, max);
        }

    }
}
