using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Sztuczna inteligencja dla klienta.
/// </summary>
public class KlientAI : MonoBehaviour
{
    #region Zmienne
    public Animator animator;
    public Strefa obecnaStrefa;
    public Sklep mozliwySklep;
    public Lawka mozliwaLawka;
    public CapsuleCollider col;
    public GameObject waypointDocelowy;
    SkinnedMeshRenderer model;
    public int numerDocelowegoWP = 0;
    public bool pomoc;
    float x = 0;

    //Poruszanie
    Vector3 margines = new Vector3(0, 0, 0);
    public int rotspeed = 2;
    public float defaultMoveSpeed = 0.4f;
    float moveSpeed = 0.4f;

    public bool siedzi = false;
    #endregion

    #region Monobehafor xD
    void Start()
    {
        col = this.gameObject.GetComponent<CapsuleCollider>();
        animator = gameObject.GetComponent<Animator>();
        model = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

        //Dodaj Rigidbody w celu wykrywania kolizji z triggerem.
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
    }

    void Update()
    {
        Animacja();
        IdzDo(waypointDocelowy);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Shop")      //Wejscie do sklepu
        {
            mozliwySklep.UsunZKolejki();
            mozliwySklep.DodajKlienta();
            model.enabled = false;
            StartCoroutine(ZacznijIscZa(10, 4));
        }

        if (other.tag == "Bench")
        {
            mozliwaLawka.UsunZKolejki();
            mozliwaLawka.DodajKlienta();
            mozliwaLawka.Posadz(this.gameObject);
            StartCoroutine(ZacznijIscZa(3, 4));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Shop")
        {
            Sklep sklep = other.gameObject.GetComponentInParent<Sklep>();
            sklep.OdejmijKlienta();
            WorldManager.instance.kasa += mozliwySklep.parent.info.neededMoney;
        }
        else if (other.tag == "Strefa")
        {
            if (other.GetComponent<Strefa>().numerStrefy == WorldManager.instance.strefy.Count-1)
            {
                Destroy(this.gameObject);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Strefa")     //Update strefy po przejściu.
        {
            //print("Wszedłem do strefy " + other.gameObject.GetComponent<Strefa>().numerStrefy);
            obecnaStrefa = other.gameObject.GetComponent<Strefa>();
            mozliwySklep = obecnaStrefa.sklep;
            mozliwaLawka = obecnaStrefa.lawka;
        }
    }
    #endregion

    #region Metody prywatne

    void IdzDo(GameObject waypoint)
    {
        if (waypoint != null && !siedzi) //Idź do
        {
            //Obrót
            Vector3 relativePos = waypoint.transform.position + margines - transform.position;
            relativePos.y = 0.0f;
            if (relativePos != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(relativePos), rotspeed);
            }
            //Przesunięcie
            transform.position = Vector3.MoveTowards(transform.position, waypoint.transform.position + margines, moveSpeed * Time.deltaTime); // Idź w stronę waypointa.
            if (transform.position == waypoint.transform.position + margines) //Czy dotarł do celu?
            {
                switch (numerDocelowegoWP) //Bazujac na tym gdzie klient wlasnie dotarl:
                {
                    case 0: //WP_IN
                        WybierzWaypoint(1);
                        break;
                    case 1: //WP_Center
                        if (mozliwySklep != null) //Sprawdź czy sklep w ogóle istnieje.
                        {
                            if (mozliwySklep.MaMiejsce())
                            {
                                mozliwySklep.UstawWKolejce();
                                WybierzWaypoint(2);
                            }
                            else //Zadecyduj czy isc na ławkę (Jeśli strefa ją posiada!) czy do WP_Out:
                            {
                                if (obecnaStrefa.lawka != null)
                                {
                                    if (obecnaStrefa.lawka.MaMiejsce())
                                    {
                                        mozliwaLawka.UstawWKolejce();
                                        WybierzWaypoint(3);
                                    }
                                    else //Jeśli ławka nie ma wolnego miejsca:
                                    {
                                        IdzDoKolejnejStrefy();
                                    }
                                }
                                else //Jeśli w strefie nie ma ławki:
                                {
                                    IdzDoKolejnejStrefy();
                                }
                            }
                        }
                        else //Jeśli sklepu nie ma idź do kolejnej strefy:
                        {
                            IdzDoKolejnejStrefy();
                        }
                        break;
                    case 2: //WP_Shop

                        break;
                    case 3: //WP_Bench

                        break;
                    case 4: //Ostatni waypoint (leży już w innej strefie)
                        WybierzWaypoint(1);
                        break;
                    default:
                        break;
                }

                //Zmień margines uwzględniając dokąd idzie klient.
                x = Random.Range(-0.5f, 0.5f);
                switch (numerDocelowegoWP) //Wylosuj nowy margines dla wybranego wczesniej waypointu, zostanie on uzyty w kolejnej klatce. 
                {
                    case 0: //WP_IN 
                        margines = new Vector3(
                            x,                        // + to góra, - to dół
                            0,                       // -------------------
                            Random.Range(0f, -0.5f) // + to "lewo", - to "prawo"
                        ); 
                        break;
                    case 2: //WP_Shop
                        margines = Vector3.zero;
                        break;
                    case 3: //WP_Bench
                        margines = new Vector3(
                            0,                           // + to góra, - to dół
                            0,                          // -------------------
                            Random.Range(-0.2f, 0.2f)  // + to "lewo", - to "prawo"
                        );
                        break;
                    case 4: //WP_Out    
                        margines = new Vector3(
                            x,                         // + to góra, - to dół
                            0,                        // -------------------
                            0                        // + to "lewo", - to "prawo"
                        );
                        break;
                    default:
                        margines = new Vector3(
                            x,                          // + to góra, - to dół
                            0,                         // -------------------
                            x                         // + to "lewo", - to "prawo"
                        );
                        break;
                }
            }
        }
        else //Jeśli strefa nie jest jeszcze wybrana/waypoint jest pusty.
        {
            obecnaStrefa = WorldManager.instance.strefy[0];
            if (obecnaStrefa.waypointy != null && obecnaStrefa.waypointy.Count > 0) //Usuwa błąd złego indeksu listy.
            {
                waypointDocelowy = obecnaStrefa.waypointy[0];
            }
        }
    }

    void IdzDoKolejnejStrefy()
    {
        WybierzWaypoint(4);
    }

    void WybierzWaypoint(int numer) //Idź do waypointu z podanym numerem. 
    {
        numerDocelowegoWP = numer;
        if (numer < obecnaStrefa.waypointy.Count)
        {
            waypointDocelowy = obecnaStrefa.waypointy[numer];
        }
        else Debug.LogWarning("Brak WP o takim numerze");
    }

    void Animacja()
    {
        if (siedzi == false)
        {
            moveSpeed = 1.2f;
            animator.SetBool("sitYorN", false);
        }
    }

    IEnumerator ZacznijIscZa(float sekundy, int numWaypointu)
    {
        yield return new WaitForSeconds(sekundy);
        UstawWidocznosc(true);
        WybierzWaypoint(numWaypointu);
    }

    #endregion

    #region Metody publiczne

    public void UstawWidocznosc(bool widoczny)
    {
        model.enabled = widoczny;
    }

    public void ZacznijIsc()
    {
        moveSpeed = defaultMoveSpeed;
    }

    public void Zatrzymaj()
    {
        moveSpeed = 0;
    }
    #endregion

}

