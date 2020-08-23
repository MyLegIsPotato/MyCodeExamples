using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// STATYCZNA Klasa zawierająca wszystkie ważne zmienne na scenie oraz metody wywoływane z wielu innych skryptów. 
/// TRZEBA:
/// -Ustawić wymiary prefabu otoczenia, WAŻNE: Prefab otocznia musi mieć ustawione waypointy w odpowiedniej kolejności w hierarchii tj. IN musi być na górze hierarchii. OUT na dole.
/// -Ustawić ilość stref na liczbę większą lub równą 1.
/// MOŻNA:
/// -Ustawić początek mapy za pomocą zmiennej origin. (Lewy górny róg pierwszej strefy).
/// </summary>

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;
    public PlayerInfo info;

    //Pokój
        //Gracz
        public string dreamName;

        //Kamera
        public GameObject mainCamera;
        public Transform cameraRoomAnchor;
        public Button goHomeButton;

        //Obiekty pokoju
        public ObiektPokoju drzwi;

    //Strefy
    [Header("Strefy")]
    public GameObject prefabOtoczeniaMiasta;
    public GameObject prefabOtoczeniaPrzedmiesci;
    public Vector3 originMiasto; //Miejsce, punkt startowy w świecie
    public Vector3 originPrzedmiescia; //Miejsce, punkt startowy w świecie
    [HideInInspector]
    public Vector3 wymiaryPrefabu;
    public int liczbaStref = 3;
    public List<Strefa> strefy;
    int wygenerowaneStrefy = 0;
    public Texture podswietlenieOkien;


    //Sklepy
    [Header("Sklepy")]
    public GameObject prefabPustegoSklepu;
    public GameObject prefabUlepszenia;
    public List<GameObject> prefabySklepow = new List<GameObject>();
    public GameObject prefabLawki;
    public GameObject prefabBudowy;


    //Spawnowanie

    //Klienci
    [Header("Klienci")]
    private Spawn spawner;
    //Samochody
    [Header("Samochody")]
    public GameObject carSpawnA;
    public GameObject carSpawnB;
    public GameObject prefabSamochodu;
    Material[] materialyKolory;

    //Czas
    [Header("Czas")]
    public Slider pasekCzasu;
    public float czas = 0;
    public float czasWolny = 6;
    public float czasTrwaniaMiesiaca_Realtime_Minutes = 15;
    public int iloscDniMiesiaca = 31;
    public float czasTrwaniaTury_Realtime_Minutes = 1;

    //Kasa 
    [Header("Kasa")]
    public Text kasatxt;
    public int kasa = 10000;
    public int goal;

    //Tryby
    [Header("Inne")]
    public string tryb = "Gra"; //Gra, Budowanie, Ulepszanie, Pokoj
    public string miejsce = "Pokoj"; //Pokoj, Miasto, Przedmiescia, Garaz


    //Pokój
    public GameObject zaczepienieMonitora;
    public GameObject ekranUI;
    public RoomDoors skryptDrzwi;


    void Awake()
    {
        ZmienMiejsce("Pokoj", null);
        ZmienTryb("Gra");
        instance = this; //Ustaw WorldManager.instance jako zmienną globalną widzianą przez wszystkie skrypty.
        spawner = GameObject.Find("Spawn").GetComponent<Spawn>();
        spawner.gameObject.SetActive(true); //Włącz spawner i zacznij spawnować ludziki.
        materialyKolory = Resources.LoadAll<Material>("MaterialyKolory");
        goHomeButton.onClick.AddListener(() => { ZmienMiejsce("Pokoj", null); });
        ZaladujSklepyZFolderu();
        GenerowanieStref(Strefa.rodzajStrefy.MIASTO);
        GenerowanieStref(Strefa.rodzajStrefy.PRZEDMIESCIA);
        AktywujSamochody();
        UstawDoMonitora();

        for (int i = 0; i<=info.expenses.Count-1; i++)
        {
            info.totalExpanses += info.expenses[i].amount;
        }
    }

    public void Update()
    {
        AktualizujCzas();
        kasatxt.text = kasa.ToString(); //Aktualizuj tekst pieniędzy.
    }

    public void UstawDoMonitora()
    {
        ekranUI.transform.SetParent(zaczepienieMonitora.transform, false);
        ekranUI.GetComponent<RectTransform>().localScale = new Vector3(0.000199f, 0.000199f, 0.000199f);
        ekranUI.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        ekranUI.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 90, 0);


    }

    public void GenerowanieStref(Strefa.rodzajStrefy rodzaj)
    {
        Vector3 origin = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        GameObject prefabOtoczenia = null; 
        if (rodzaj == Strefa.rodzajStrefy.MIASTO)
        {
            origin = originMiasto;
            rotation = Quaternion.identity;
            prefabOtoczenia = prefabOtoczeniaMiasta;
        }
        else if(rodzaj == Strefa.rodzajStrefy.PRZEDMIESCIA)
        {
            origin = originPrzedmiescia;
            rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            prefabOtoczenia = prefabOtoczeniaPrzedmiesci;

        }

        for (; wygenerowaneStrefy < liczbaStref; wygenerowaneStrefy++) //Stwórz <liczbaStref> stref.
        {
            wymiaryPrefabu = prefabOtoczenia.GetComponentInChildren<MeshRenderer>().bounds.size;
            Vector3 pozycja = origin - new Vector3(0, 0, wymiaryPrefabu.z * wygenerowaneStrefy);
            GameObject nowaStrefa = Instantiate(prefabOtoczenia, pozycja, rotation);
            nowaStrefa.name = rodzaj.ToString().ToLower() + " #" + wygenerowaneStrefy.ToString();
            strefy.Add(nowaStrefa.AddComponent<Strefa>()); //Stwórz nowy obiekt skryptu Strefa i dodaj go do listy w WorldManagerze.
            strefy[wygenerowaneStrefy].numerStrefy = wygenerowaneStrefy; //Przypisz indeks stworzonej strefie
        }
        wygenerowaneStrefy = 0;
        for (; wygenerowaneStrefy < 4; wygenerowaneStrefy++) //Dodaj puste strefy po lewej.
        {
            wymiaryPrefabu = prefabOtoczenia.GetComponentInChildren<MeshRenderer>().bounds.size;
            Vector3 pozycja = origin + new Vector3(0, 0, wymiaryPrefabu.z * wygenerowaneStrefy);
            GameObject nowaStrefa = Instantiate(prefabOtoczenia, pozycja, rotation);
            nowaStrefa.name = rodzaj.ToString().ToLower() + " Filler";
        }
        wygenerowaneStrefy = 0;
        UnityEngine.Camera.main.GetComponent<CameraController>().UstawLimit(transform.position.z + 3f, transform.position.z - 50f);
    }

    public void KupStrefe()
    {
        Vector3 pozycja = originMiasto - new Vector3(0, 0, wymiaryPrefabu.z * liczbaStref);
        GameObject nowaStrefa = Instantiate(prefabOtoczeniaMiasta, pozycja, Quaternion.identity);
        nowaStrefa.name = "Strefa #" + liczbaStref.ToString();
        strefy.Add(nowaStrefa.AddComponent<Strefa>()); //Stwórz nowy obiekt skryptu Strefa i dodaj go do listy w WorldManagerze.
        strefy[liczbaStref].numerStrefy = liczbaStref; //Przypisz indeks stworzonej strefie
        liczbaStref++;
        carSpawnB.transform.position += new Vector3(0, 0, -wymiaryPrefabu.z); //Przesuń spawn auta B
    }

    public void AktywujSamochody()
    {
        //Ustaw Waypointy
        GameObject waypointA = new GameObject();
        waypointA.name = "O_SpawnSamochoduA";
        waypointA.transform.position = originMiasto + new Vector3(-wymiaryPrefabu.x + 0.30f, 0, originMiasto.z + 6);
        waypointA.transform.parent = this.transform;
        GameObject waypointB = new GameObject();
        waypointB.name = "O_SpawnSamochoduB";
        waypointB.transform.position = originMiasto + new Vector3(-wymiaryPrefabu.x + 1.30f, 0, originMiasto.z - (wymiaryPrefabu.z*liczbaStref));
        waypointB.transform.parent = this.transform;
        carSpawnA = waypointA;
        carSpawnB = waypointB;

        SpawnujSamochod(true); //B
        SpawnujSamochod(false); //A

    }

    public void SpawnujSamochod(bool B)
    {
        //print("Spawnuje samochody");
        //Spawnuj samochody
        //A
        if (!B)
        {
            GameObject samochodA = Instantiate(prefabSamochodu);
            samochodA.name = "Samochód A";
            Util.LosujKolor(samochodA.GetComponentInChildren<MeshRenderer>(), materialyKolory);
            samochodA.transform.position = carSpawnA.transform.position;
            samochodA.AddComponent<CarMovement>();
        }
        else
        {
            //B
            GameObject samochodB = Instantiate(prefabSamochodu);
            samochodB.name = "Samochód B";
            Util.LosujKolor(samochodB.GetComponentInChildren<MeshRenderer>(), materialyKolory);
            CarMovement b = samochodB.AddComponent<CarMovement>();
            b.B = true;
        }
    }

    public void ZmienTryb(string nazwa)
    {
        tryb = nazwa;
        switch (tryb)
        {
            case "Gra":
                //Time.timeScale = 1f;
                foreach (Strefa strefa in strefy)
                {
                    strefa.SchowajDucha();
                }
                break;
            case "Ulepszanie":
                //?
                break;
            case "Budowanie":
                //Time.timeScale = 0.2f;
                foreach (Strefa strefa in strefy)
                {
                    strefa.PokazDucha();
                }
                break;
            default:
                Debug.LogWarning("Brak takiego trybu: " + tryb);
                break;
        }

    }

    public void ZmienMiejsce(string nazwa, Transform transformMiejsca)
    {
        miejsce = nazwa;
        print("Zmieniam miejsce na: " + miejsce);
        KontrolerInterfejsu.instance.mouseBlocker.SetActive(false);
        switch (miejsce)
        {
            case "Pokoj":
                drzwi.percentProgress = 0;
                drzwi.Oddal();
                KontrolerInterfejsu.instance.SchowajElementy(KontrolerInterfejsu.presety.POKOJ);
                Camera.main.transform.SetParent(cameraRoomAnchor,true);
                Camera.main.transform.localPosition = Vector3.zero;
                Camera.main.transform.localRotation = Quaternion.Euler(0, 0, 0);
                Camera.main.transform.localScale = Vector3.one;
                break;
            case "Miasto":
                Camera.main.transform.SetParent(null);
                Camera.main.transform.position = transformMiejsca.position;
                Camera.main.transform.rotation = transformMiejsca.rotation;
                Camera.main.transform.localScale = transformMiejsca.localScale;
                Camera.main.fieldOfView = 69;
                break;
            case "Przedmiescia":
                Camera.main.transform.SetParent(null);
                Camera.main.transform.position = transformMiejsca.position;
                Camera.main.transform.rotation = transformMiejsca.rotation;
                Camera.main.transform.localScale = transformMiejsca.localScale;
                Camera.main.fieldOfView = 69;
                break;
            case "Garaz":
                Camera.main.transform.SetParent(null);
                Camera.main.transform.position = transformMiejsca.position;
                Camera.main.transform.rotation = transformMiejsca.rotation;
                Camera.main.transform.localScale = transformMiejsca.localScale;
                Camera.main.fieldOfView = 55;
                break;
            default:
                Debug.LogWarning("Brak takiego miejsca: " + miejsce);
                break;
        }
    }

    void ZaladujSklepyZFolderu()
    {
        GameObject[] temp = Resources.LoadAll<GameObject>("PrefabySklepy");
        foreach (var sklep in temp)
        {
            string nazwa = sklep.name;
            switch (nazwa) //Sklepy specjalne np. pusty, budowa etc.
            {
                case "O_Unknown":
                    prefabPustegoSklepu = sklep;
                    break;
                case "O_Construction":
                    prefabBudowy = sklep;
                    break;
                case "O_arrow":
                    prefabUlepszenia = sklep;
                    break;
                default: //Wszystkie normalne sklepy:
                    prefabySklepow.Add(sklep);
                    break;
            }
        }
    }

    public void DezaktywujDuchy()
    {
        foreach (Strefa strefa in strefy)
        {
            strefa.SchowajDucha();
        }
    }

    public void AktywujDuchy()
    {
        foreach (Strefa strefa in strefy)
        {
            strefa.PokazDucha();
        }
    }

    public void AktualizujCzas()
    {
        czasTrwaniaTury_Realtime_Minutes = (czasWolny / 24) * czasTrwaniaMiesiaca_Realtime_Minutes;
        czas = Time.time;
        //print(czas);
        pasekCzasu.maxValue = czasTrwaniaTury_Realtime_Minutes;
        pasekCzasu.value = czas/60;
    }
}
