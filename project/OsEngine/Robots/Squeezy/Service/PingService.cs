using OsEngine.Robots.Squeezy.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.Robots.Squeezy.Service
{

    public class PingService
    {
        private static PingService instance;

        private TgService tgService;

        private Thread threadPing;
        private const int PING_TIME_OUT = 1800000;//30 минут
        private PingService(bool isEnabled, TgService tgService)
        {
            this.tgService = tgService;
            if (!isEnabled)
            {
                return;
            }

            threadPing = new Thread(new ThreadStart(ping));
            threadPing.IsBackground = true;
            threadPing.SetApartmentState(ApartmentState.STA);
            threadPing.Start();
        }

        public static PingService getInstance(bool isEnabled, TgService tgService)
        {
            if (instance == null)
            {
                instance = new PingService(isEnabled, tgService);
            }
            return instance;
        }

        private void ping()
        {
            while (true)
            {
                tgService.sendPing();
                Thread.Sleep(PING_TIME_OUT);
            }
        }
    }
}
