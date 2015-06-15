using ECQRS.Commons.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECQRS.Commons.Events
{
    public interface IEventView : IEventHandler, IECQRSService
    {
    }
}
