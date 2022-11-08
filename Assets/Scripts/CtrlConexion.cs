using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class CtrlConexion : MonoBehaviourPunCallbacks
{
    #region Variables privadas
    #endregion


    #region Variables publicas
    public GameObject panelInicio; // Panel de inicio del juego
    public GameObject panelBienvenida; // Panel de bienvenida del juego
    public GameObject panelCreacionSala; // Panel para creación de una sala
    public GameObject panelSala; // Panel de la sala
    public GameObject contenedor; // Contenedor del boton de jugadores
    public GameObject cajaJugador; // Caja de jugador para la sala

    public TMP_InputField inputNickname; // Entrada con el nombre de usuario
    public TMP_InputField inputNombreSala; // El usuario introduce el nombre de la sala
    public TMP_InputField inputMinJug; // El usuario indica el minimo de jugadores
    public TMP_InputField inputMaxJug; // El usuario indica el maximo de jugadores permitidos

    public TextMeshProUGUI txtEstado;  // Contiene la última salida por pantalla en estado
    public TextMeshProUGUI txtInfoUser;  // Contiene información sobre el usuario
    public TextMeshProUGUI nombreSala; // Nombre de la sala
    public TextMeshProUGUI tipoSala; // Muestra el tipo de sala
    public TextMeshProUGUI totalJugadores; // Muestra los jugadores actuales en la sala
    public TextMeshProUGUI maximoJugadores; // Muestra el máximo de jugadores de la sala

    public Button botonConectar; // Instancia del boton Conectar
    

    #endregion

    private void Start()
    {
        CambiarPanel(panelInicio);
    }



    #region Eventos para botones
    /// <summary>
    /// Método que se ejecuta al pulsar el botón de Conexión a Photon
    /// Comprueba si el nombre de usuario es correcto y realiza la conexión
    /// </summary>
    public void OnClickConectarAServidor()
    {
        // Comprobamos si el nombre de usuario es correcto
        if (!(string.IsNullOrWhiteSpace(inputNickname.text) ||
            string.IsNullOrEmpty(inputNickname.text)))
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
                CambiarEstado("Ya está conectado a Photon");
            }
        } else
        {
            // Indicar que el nombre no es correcto
            CambiarEstado("Nombre de usuario incorrecto");
        }
    }

    /// <summary>
    /// Método que se lanza al pulsar el botón Crear Sala del 
    /// menú de bienvenida. Cambia al panel CreacionSala
    /// </summary>
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

        if (!(string.IsNullOrWhiteSpace(inputNombreSala.text) ||
            string.IsNullOrEmpty(inputNombreSala.text)))
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

    #endregion

    #region Eventos propios de Photon
    public override void OnConnected()
    {
        //base.OnConnected();
        CambiarEstado("Conectado a Photon");
        PhotonNetwork.NickName = inputNickname.text;
        txtInfoUser.text = PhotonNetwork.NickName;
        // Cambiamos a panelBienvenida
        CambiarPanel(panelBienvenida);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        CambiarEstado("Sala creada correctamente");
        CambiarPanel(panelSala);
        
        // Información de los jugadores en sala
        Player[] jugadores = PhotonNetwork.PlayerList;
        foreach (var jugador in jugadores)
        {
            GameObject botonNuevo = Instantiate(cajaJugador);
            botonNuevo.transform.SetParent(contenedor.transform);
            botonNuevo.transform.Find("Nickname").GetComponent<TextMeshProUGUI>().text = jugador.NickName;

            //cajaJugador.GetComponentInChildren<TextMeshProUGUI>().text = jugador.NickName;
            //jugadoresEnSala.text = jugador.ActorNumber.ToString();
            //jugadoresEnSala.text = jugador.NickName.ToString();
        }
        
        // Información de la Sala
        nombreSala.text = "Sala " + PhotonNetwork.CurrentRoom.Name.ToString();
        tipoSala.text = (PhotonNetwork.CurrentRoom.IsOpen ? "Abierta" : "Cerrada");
        totalJugadores.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        maximoJugadores.text = PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
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

        panelObjetivo.SetActive(true);
    }
    #endregion
}
