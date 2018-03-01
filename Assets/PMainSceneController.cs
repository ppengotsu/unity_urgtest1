using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Net.Sockets;
using SCIP_library;

namespace Test1{
public class PMainSceneController : MonoBehaviour {

	[SerializeField]
	private InputField  ipAddressInputField;

		[SerializeField]
	private InputField  portInputField;

	[SerializeField]
	private InputField  outInputField;

	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void StartButton(){

		Debug.Log("開始");
		const int GET_NUM = 10;
        const int start_step = 0;
        const int end_step = 760;
		try {
			
            string ip_address = this.ipAddressInputField.text;
            int port_number = int.Parse(this.portInputField.text);
            

			this.outInputField.text += ip_address + " : " + port_number + "\n";
			Debug.Log(ip_address + " : " + port_number );
            
			
			TcpClient urg = new TcpClient();
            urg.Connect(ip_address, port_number);
            NetworkStream stream = urg.GetStream();

            write(stream, SCIP_Writer.SCIP2());
            read_line(stream); // ignore echo back
            write(stream, SCIP_Writer.MD(start_step, end_step));
            read_line(stream);  // ignore echo back

            List<long> distances = new List<long>();
            long time_stamp = 0;
            for (int i = 0; i < GET_NUM; ++i) {
                string receive_data = read_line(stream);
                if (!SCIP_Reader.MD(receive_data, ref time_stamp, ref distances)) {
					this.outInputField.text += receive_data + "\n";
                   Debug.Log(receive_data);
                    break;
                }
                if (distances.Count == 0) {
                    this.outInputField.text += receive_data + "\n";
					Debug.Log(receive_data);
                    continue;
                }
                // show distance data
                //Console.WriteLine("time stamp: " + time_stamp.ToString() + " distance[100] : " + distances[100].ToString());
                for(int j =0; j < distances.Count; j++){
				//this.outInputField.text += "\ntime stamp: " + time_stamp.ToString() + " distance[" + j + "] : " + distances[j].ToString() + "\n";
					Debug.Log("time stamp: " + time_stamp.ToString() + " distance[" + j + "] : " + distances[j].ToString() );
                }
            }
            write(stream, SCIP_Writer.QT());    // stop measurement mode
            read_line(stream); // ignore echo back
            stream.Close();
            urg.Close();
        } catch (Exception ex) {
            Debug.Log(ex.Message);
            Debug.Log(ex.StackTrace);
        } finally {
            Debug.Log("Finish");
            //Console.ReadKey();
        }
    }


    /// <summary>
    /// Read to "\n\n" from NetworkStream
    /// </summary>
    /// <returns>receive data</returns>
     string read_line(NetworkStream stream)
    {
        if (stream.CanRead) {
            StringBuilder sb = new StringBuilder();
            bool is_NL2 = false;
            bool is_NL = false;
            do {
                char buf = (char)stream.ReadByte();
                if (buf == '\n') {
                    if (is_NL) {
                        is_NL2 = true;
                    } else {
                        is_NL = true;
                    }
                } else {
                    is_NL = false;
                }
                sb.Append(buf);
            } while (!is_NL2);

            return sb.ToString();
        } else {
            return null;
        }
	}

	
    /// <summary>
    /// write data
    /// </summary>
     bool write(NetworkStream stream, string data)
    {
        if (stream.CanWrite) {
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            stream.Write(buffer, 0, buffer.Length);
            return true;
        } else {
            return false;
        }
    }
}
}

