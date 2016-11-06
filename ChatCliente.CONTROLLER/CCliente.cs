using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatCliente.CONTROLLER
{
    public class CCliente
    {
        //VARIÁVEIS GLOBAIS

        private StreamWriter Transmissor;


        //MÉTODOS

        public void Conectar(string ip, string usuario)
        {
            if (!usuario.Equals(""))
            {
                try
                {
                    // Socket de conexão ao servidor
                    TcpClient ServidorSocket = new TcpClient();
                    ServidorSocket.Connect(IPAddress.Parse(ip), 2502);

                    Transmissor = new StreamWriter(ServidorSocket.GetStream());
                    // Código 01: Tentativa de conexão.
                    // Após o envio do código, o servidor deve retornar o mesmo para confirmar a conexão.
                    Transmissor.WriteLine("01|"+usuario);
                    Transmissor.Flush();
                    ServidorSocket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }

            //
            Thread t = new Thread(RunDelay);
            t.Start();
            t.Join();
        }

        public void EnviarMensagem(string mensagem)
        {
            Transmissor.WriteLine(mensagem);
            Transmissor.Flush();
        }

        //
        private static void RunDelay()
        {
            Thread.Sleep(1000);
        }
    }
}
