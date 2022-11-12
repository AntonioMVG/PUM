using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;


public class CtrlConexion : MonoBehaviourPunCallbacks
{
    #region Variables publicas
    public GameObject panelInicio;
    public GameObject panelBienvenida;
    public GameObject panelCreacionSala;
    public GameObject panelSala;
    public GameObject panelUnirseSala;
    public GameObject contenedor;
    public GameObject cajaJugador;

    public TMP_InputField inputNickname;
    public TMP_InputField inputNombreSala;
    public TMP_InputField inputSalaUnirse;
    public TMP_InputField inputMinJug;
    public TMP_InputField inputMaxJug;

    public TextMeshProUGUI txtEstado;
    public TextMeshProUGUI txtInfoUser;
    public TextMeshProUGUI nombreSala;
    public TextMeshProUGUI tipoSala;
    public TextMeshProUGUI totalJugadores;
    public TextMeshProUGUI maximoJugadores;

    public Button botonConectar;
    #endregion

    private void Start()
    {
        CambiarPanel(panelInicio);
    }

    #region Eventos para botones
    public void OnClickConectarAServidor()
    {
        // Comprobamos si el nombre de usuario es correcto
        if (!(string.IsNullOrWhiteSpace(inputNickname.text)))
        {
            // Comprobamos si no estamos ya conectados a Photon
            if (!(PhotonNetwork.IsConnected))
            {
                // Deshabilitamos el botón para evitar doble pulsación
                botonConectar.interactable = false;
                // Conectamos a Photon con la configuración de usuario
                PhotonNetwork.ConnectUsingSettings(); 
                
                CambiarEstado("Conectando...");
            }
            else
            {
                // Indicar que ya estamos conectados
                CambiarEstado("Conectado a Photon");
            }
        }
        else
        {
            // Indicar que el nombre no es correcto
            CambiarEstado("Nombre de usuario incorrecto");
        }
    }

    public void OnClickIrACrearSala()
    {
        CambiarPanel(panelCreacionSala);
    }

    /// <summary>
    /// Método que se lanza al pulsar el botón 
    /// Crear Sala del panel Creación de sala.
    /// Este método comprueba que el nombre de la sala es válido
    /// y que los valores para el número de jugadores son correctos
    /// </summary>
    public void OnClickCrearSala()
    {
        // Empezamos comprobando el nombre de sala. 
        // Si es válido, comprobamos los valores para el número de jugadores.
        // Sólo entonces, se crea la sala con los valores indicados.

        int min, max;
        min = int.Parse(inputMinJug.text);
        max = int.Parse(inputMaxJug.text);

        if (!(string.IsNullOrWhiteSpace(inputNombreSala.text)))
        {
            if (min>0 && max >= min)
            {
                RoomOptions opcionesSala = new RoomOptions();
                opcionesSala.MaxPlayers = (byte) max;
                opcionesSala.IsVisible = true;
                opcionesSala.IsOpen = false;

                PhotonNetwork.CreateRoom(inputNombreSala.text, opcionesSala, TypedLobby.Default);
            }
            else
            {
                CambiarEstado("Número de jugadores no válido");
            }
        }
        else
        {
            CambiarEstado("Nombre de sala incorrecto");
        }
    }

    public void OnClickUnirseASala()
    {
        CambiarPanel(panelUnirseSala);
    }
    #endregion

    #region Eventos propios de Photon
    public override void OnConnected()
    {
        //base.OnConnected();
        CambiarEstado("Conectado a Photon");
        PhotonNetwork.NickName = inputNickname.text;
        txtInfoUser.text = PhotonNetwork.NickName;
        CambiarPanel(panelBienvenida);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        CambiarEstado("Conectado a Photon");
        PhotonNetwork.NickName = inputNickname.text;
        txtInfoUser.text = PhotonNetwork.NickName;
        CambiarPanel(panelBienvenida);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        CambiarEstado("Sala creada correctamente");
        CambiarPanel(panelSala);
        
        // Actualizamos los datos de la sala y los jugadores
        ActualizarSala();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        CambiarEstado("Dentro de la sala");
        PhotonNetwork.JoinRoom(inputSalaUnirse.text);
        CambiarPanel(panelSala);
        //ActualizarSala();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            CambiarEstado("Sala Completa");
        }
        else
        {
            ActualizarSala();
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.LeaveRoom();
        CambiarEstado("Has abandonado la sala");
        CambiarPanel(panelBienvenida);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        CambiarEstado("Error al crear sala: " + message);
    }
    #endregion


    #region Métodos privados
    /// <summary>
    /// Método que cambiará el mensaje de Estado 
    /// de los paneles de introducción al juego
    /// </summary>
    /// <param name="texto">Nuevo mensaje a colocar</param>
    private void CambiarEstado (string texto)
    {
        txtEstado.text = texto;
    }

    private void CambiarPanel (GameObject panelObjetivo)
    {
        panelBienvenida.SetActive(false);
        panelInicio.SetActive(false);
        panelCreacionSala.SetActive(false);
        panelSala.SetActive(false);
        panelUnirseSala.SetActive(false);

        panelObjetivo.SetActive(true);
    }

    private void ActualizarSala()
    {
        // Información de los jugadores en sala
        Player[] jugadores = PhotonNetwork.PlayerList;
        foreach (var jugador in jugadores)
        {
            GameObject botonNuevo = Instantiate(cajaJugador, contenedor.transform);
            //botonNuevo.transform.SetParent(contenedor.transform);
            botonNuevo.transform.Find("Nickname").GetComponent<TextMeshProUGUI>().text = jugador.NickName;
        }

        // Información de la Sala
        nombreSala.text = "Sala " + PhotonNetwork.CurrentRoom.Name.ToString();
        tipoSala.text = (PhotonNetwork.CurrentRoom.IsOpen ? "Abierta" : "Cerrada");
        totalJugadores.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/";
        maximoJugadores.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }

    #endregion

    #region Métodos públicos
    public void DesconectarServidor()
    {
        botonConectar.interactable = true;
        inputNickname.text = string.Empty;
        txtInfoUser.text = "No user";
        CambiarPanel(panelInicio);
        CambiarEstado("Desconectado de Photon");
        PhotonNetwork.Disconnect();
    }

    public void UnirseASala()
    {
        
        if (!(string.IsNullOrWhiteSpace(inputSalaUnirse.text)))
        {
            PhotonNetwork.JoinRoom(inputSalaUnirse.text);
            CambiarPanel(panelSala);
        }
    }

    public void VolverAtras()
    {
        CambiarPanel(panelBienvenida);
    }
    #endregion
}
