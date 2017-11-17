using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NFVO.Models;
using System.Collections;

namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TackerAPI t = new TackerAPI();
            // VimModels vim = t.RegisterVim();
            t.CreateVnfd();
        }
    }
}
