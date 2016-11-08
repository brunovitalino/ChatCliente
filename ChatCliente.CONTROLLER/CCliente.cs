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

        private string _ip;
        private string _usuario;
        private bool _conectado;

        public string Ip
        {
            get { return _ip; }
            set { _ip = value; }
        }

        public string Usuario
        {
            get { return _usuario; }
            set { _usuario = value; }
        }

        public bool Conectado
        {
            get { return _conectado; }
            set { _conectado = value; }
        }


        //CONSTRUTOR

        public CCliente()
        {
        }

        public CCliente(string ip, string usuario)
        {
            Ip = ip;
            Usuario = usuario;
            Conectado = false;
        }


        //MÉTODOS

        public void Conectar(bool conectar)
        {
            Console.WriteLine("thread1 in (Conectado:" + Conectado + ")");

            if (conectar)
            {
                if (!Usuario.Equals(""))
                {
                    Conectado = true;
                    // Inicia uma nova tread que fará nossa conexão.
                    Thread ThreadConectar = new Thread(RunConectar);
                    ThreadConectar.Start();
                }
            }
            else
            {
                Conectado = false;
            }

            //
            Thread t = new Thread(RunDelay);
            t.Start();
            t.Join();

            Console.WriteLine("thread1 out (Conectado:" + Conectado + ")");
        }

        private void RunConectar()
        {
            Console.WriteLine("thread2 in (Conectado:" + Conectado + ")");

            try
            {
                // Socket de conexão ao servidor
                TcpClient novoServidorSocket = new TcpClient();
                novoServidorSocket.Connect(IPAddress.Parse(Ip), 2502);

                StreamWriter Transmissor = new StreamWriter(novoServidorSocket.GetStream());
                // Código 01: Tentativa de conexão.
                // Após o envio do código, o servidor deve retornar o mesmo para confirmar a conexão.
                Transmissor.WriteLine("01|" + Usuario);
                Transmissor.Flush();
                Console.WriteLine("01");

                //Mantém a conexão.
                while (true)
                {
                    Thread.Sleep(1500);
                    Console.WriteLine("Conectado: "+Conectado);

                    if (!Conectado)
                    {
                        Console.WriteLine("Saindo!");
                        break;
                    }
                }
                // As variáveis locais do try provavelmenta já vão para a fila do garbage, mas colocamos as próximas duas linhas para garantir fechamentyo de conexão.
                Transmissor.Close();
                novoServidorSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            Console.WriteLine("thread2 out (Conectado:" + Conectado + ")");
        }

        /*public void EnviarMensagem(string mensagem)
        {
            Transmissor.WriteLine(mensagem);
            Transmissor.Flush();
        }*/

        //
        private static void RunDelay()
        {
            Thread.Sleep(1500);
        }
    }
}
