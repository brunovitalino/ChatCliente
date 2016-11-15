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
    //DELEGATES

    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);


    //CLASSE

    public class CCliente
    {
        //VARIÁVEIS GLOBAIS

        private string _ipServidor;
        private string _usuario;
        private bool _conectado;
        private TcpClient ServidorSocket;
        // Recptor global para que possamos fazer o controle através de outra thread externa.
        private StreamReader Receptor;
        private StreamWriter Transmissor;

        public string IpServidor
        {
            get { return _ipServidor; }
            set { _ipServidor = value; }
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

        public CCliente(string ipServidor, string usuario)
        {
            IpServidor = ipServidor;
            Usuario = usuario;
            Conectado = false;
            ServidorSocket = null;
            Receptor = null;
            Transmissor = null;
        }

        //EVENTOS

        public static event StatusChangedEventHandler StatusChanged;


        //MÉTODOS

        public static void OnStatusChanged(StatusChangedEventArgs e)
        {
            if (StatusChanged != null)
            {
                StatusChanged(null, e);
            }
        }

        public void Conectar(bool conectar)
        {
            Console.WriteLine("thread1 in (Conectado:" + Conectado + ")");
            if (conectar && !Conectado)
            {
                if (!Usuario.Equals(""))
                {
                    Thread ThreadConectar = new Thread(RunConectar);
                    ThreadConectar.Start();
                }
            }
            else
            {
                Conectado = false;
            }
            Console.WriteLine("thread1 out (Conectado:" + Conectado + ")");
        }


        public void RunConectar()
        {
            Console.WriteLine("thread1.1 in (Conectado:" + Conectado + ")");
            // Inicia uma nova tread que fará nossa conexão.
            Thread ThreadCliente = new Thread(RunCliente);
            ThreadCliente.Start();

            // Se dentro de 2,5s não receber resposta, desconecte.
            int tempoLimite = 0;
            Console.WriteLine("COMECOU ESPERA");

            // Tratamento da tentativa de conexão.
            while (true)
            {
                if (Conectado)
                {
                    Console.WriteLine("conectado!");
                    break;
                }

                Thread.Sleep(100);
                tempoLimite += 100;

                //Console.WriteLine("tempo limite: " + tempoLimite);
                if (tempoLimite >= 2500)
                {
                    Console.WriteLine("Esgotado tempo limite: " + tempoLimite);
                    Console.WriteLine("desconectado!");
                    Conectado = false;
                    break;
                }
            }

            // Tratamento de desconexão. Libera os recursos para permitir o fechamento do aplicativo.
            if (Conectado)
            {
                while (Conectado)
                {
                    // Recursos em uso.
                }
                // Após a desconexão, o laço anterior será finalizado e os recursos poderão ser liberados.
                if (Receptor != null)
                {
                    //Receptor.Close();
                }
            }

            Console.WriteLine("TERMINO ESPERA");
            Console.WriteLine("thread1.1 out (Conectado:" + Conectado + ")");
        }

        public void RunCliente()
        {
            Console.WriteLine("thread2 in (Conectado:" + Conectado + ")");
            try
            {
                // Socket de conexão ao servidor
                ServidorSocket = new TcpClient();
                // Se o destino não existir, uma SocketException já será disparada automaticamente. Assim, Conectado ainda continuará false.
                ServidorSocket.Connect(IPAddress.Parse(IpServidor), 2502);
                Conectado = true;

                Transmissor = new StreamWriter(ServidorSocket.GetStream());
                string resposta = "";
                Receptor = new StreamReader(ServidorSocket.GetStream());

                // Código 01: Tentativa de conexão.
                // Após o envio do código, o servidor deve retornar o mesmo para confirmar a conexão.

                Transmissor.WriteLine("01|" + Usuario);
                Transmissor.Flush();
                Console.WriteLine("Enviou 01");

                resposta = Receptor.ReadLine();
                if (resposta.Substring(0, 2).Equals("01"))
                {
                    Console.WriteLine("Recebeu 01 CONECTADO!");
                    OnStatusChanged(new StatusChangedEventArgs("Conectado callback!"));

                    // Mantém a conexão.
                    while (true)
                    {
                        Thread.Sleep(500);

                        // Código 00: Desconexão.
                        if (!Conectado || resposta.Substring(0, 2).Equals("00"))
                        {
                            Transmissor.WriteLine("00");
                            Transmissor.Flush();
                            break;
                        }
                        // Código 10: Mensagem.
                        else if (resposta.Substring(0, 2).Equals("10"))
                        {
                            OnStatusChanged(new StatusChangedEventArgs(resposta.Substring(3)));
                        }
                        else if (resposta.Substring(0, 2).Equals("11"))
                        {
                            // ~~
                        }
                        else
                        {
                            Transmissor.WriteLine("01");
                            Transmissor.Flush();
                        }

                        resposta = Receptor.ReadLine();
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("socket destino inalcançável");
            }
            catch (IOException e)
            {
                Console.WriteLine("resposta destino inexistente");
            }
            finally
            {
                if (ServidorSocket != null)
                {
                    ServidorSocket.Close();
                }

                Console.WriteLine("finnaly");
                Conectado = false;
            }
            Console.WriteLine("thread2 out (Conectado:" + Conectado + ")");
        }

        public void EnviarMensagem(string mensagem)
        {
            Transmissor.WriteLine("10|" + mensagem);
            Transmissor.Flush();
        }

        //
        private static void RunDelay()
        {
            Thread.Sleep(3000);
        }
    }
}
