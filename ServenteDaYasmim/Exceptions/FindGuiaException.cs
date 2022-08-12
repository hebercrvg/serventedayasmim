using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServenteDaYasmim.Exceptions
{
    public class FindGuiaException : Exception
    {
        public FindGuiaException() : base("Guia já faturada ou não foi encontrada.")
        {
            
        }
    }
}
