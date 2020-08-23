using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KoloCzasu : MonoBehaviour
{
    public static KoloCzasu instance;

    public float czasPracy
    {
        get { return _czasPracy; }
        set
        {
            _czasPracy = Mathf.Clamp(value, 0, 24);
            //print("Wartość \"czas\" została zmieniona! Nowa wartość: " + _czasPracy);
            WartoscZmieniona(_czasPracy);
        }
    }

    public float czasSen
    {
        get { return _czasSen; }
        set
        {
            _czasSen = Mathf.Clamp(value, 0, 24);
            //print("Wartość \"czas\" została zmieniona! Nowa wartość: " + _czasSen);
            WartoscZmieniona(_czasSen);
        }
    }

    public float czasZdrowie
    {
        get { return _czasZdrowie; }
        set
        {
            _czasZdrowie = Mathf.Clamp(value, 0, 24);
            //print("Wartość \"czas\" została zmieniona! Nowa wartość: " + _czasZdrowie);
            WartoscZmieniona(_czasZdrowie);
        }
    }

    public float czasWolny
    {
        get { return _czasWolny; }
        set
        {
            _czasWolny = Mathf.Clamp(value, 0, 24);
            WorldManager.instance.czasWolny = _czasWolny;
            //print("Wartość \"czas\" została zmieniona! Nowa wartość: " + _czasWolny);
        }
    }

    [SerializeField]
    private float _czasPracy = 6;
    [SerializeField]
    private float _czasSen = 6;
    [SerializeField]
    private float _czasZdrowie = 6;
    [SerializeField]
    private float _czasWolny = 6;



    public List<Slider> suwaki = new List<Slider>();
    public List<GameObject> wskaznikiPivoty = new List<GameObject>();
    public List<GameObject> wskazniki = new List<GameObject>();

    public GameObject wycinekPraca;
    public GameObject wycinekSen;
    public GameObject wycinekZdrowie;
    public GameObject wycinekCzasWolny;

    void Awake()
    {
        instance = this;
        Aktualizuj();
    }

    public void Aktualizuj()
    {
        czasPracy = suwaki[0].value;
        czasSen = suwaki[1].value;
        czasWolny = 24 - czasPracy - czasSen - czasZdrowie;
    }

    public void WartoscZmieniona(float wartosc)
    {
        if(czasPracy + czasSen + czasZdrowie < 24 && wartosc < 24) //Jeśli te 3 nie zajmują całego dnia
        {
            czasWolny = 24-(czasSen+czasPracy+czasZdrowie);

            //Praca
            wycinekPraca.GetComponent<Image>().fillAmount = czasPracy / 24;
            wycinekPraca.GetComponent<RectTransform>().transform.rotation = Quaternion.Euler(0, 0, 0);
            //Praca
            wskaznikiPivoty[0].GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, (360 / 48) * -czasPracy);
            wskazniki[0].GetComponentInChildren<RectTransform>().localRotation = Quaternion.Euler(0, 0, (360 / 48) * czasPracy);


            //Sen
            wycinekSen.GetComponent<Image>().fillAmount = czasSen / 24;
            wycinekSen.GetComponent<RectTransform>().transform.rotation = Quaternion.Euler(0, 0, ((360 / 24) * -czasPracy));
            //Sen
            wskaznikiPivoty[1].GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, ((360 / 24) * -czasPracy) + ((360 / 48) * -czasSen)); //Działa
            wskazniki[1].GetComponentInChildren<RectTransform>().localRotation = Quaternion.Euler(0, 0, ((360 / 24) * czasPracy) + ((360 / 48) * czasSen));


            //Zdrowie
            wycinekZdrowie.GetComponent<Image>().fillAmount = czasZdrowie / 24;
            wycinekZdrowie.GetComponent<RectTransform>().transform.rotation = Quaternion.Euler(0, 0, ((360 / 24) * -czasPracy) + ((360 / 24) * -czasSen));
            //Zdrowie
            wskaznikiPivoty[2].GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, ((360 / 24) * -czasPracy) + ((360 / 24) * -czasSen) + ((360 / 48) * -czasZdrowie));
            wskazniki[2].GetComponentInChildren<RectTransform>().localRotation = Quaternion.Euler(0, 0, ((360 / 24) * czasPracy) + ((360 / 24) * czasSen) + ((360 / 48) * czasZdrowie));


            //Wolny
            wycinekCzasWolny.GetComponent<Image>().fillAmount = czasWolny / 24;
            wycinekCzasWolny.GetComponent<RectTransform>().transform.rotation = Quaternion.Euler(0, 0, ((360 / 24) * -czasPracy) + ((360 / 24) * -czasSen) + ((360 / 24) * -czasZdrowie));
            //Wolny
            wskaznikiPivoty[3].GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, ((360 / 24) * -czasPracy) + ((360 / 24) * -czasSen) + ((360 / 24) * -czasZdrowie) +((360 / 48) * -czasWolny));
            wskazniki[3].GetComponentInChildren<RectTransform>().localRotation = Quaternion.Euler(0, 0, ((360 / 24) * czasPracy) + ((360 / 24) * czasSen) + ((360 / 24) * czasZdrowie) + (360 / 48) * czasWolny);

            foreach (GameObject pivot in wskaznikiPivoty)
            {
                PrzerzucWPoziomie(pivot);
            }

        }
        else
        {
            Debug.LogWarning("Przekroczono ilość wykorzystanych godzin na dobę!"); //Nie usuwać!
        }
    }

    void PrzerzucWPoziomie(GameObject pivot)
    {
        RectTransform rTransform = pivot.GetComponent<RectTransform>();
        print(string.Format("Obrót w osi Z obiektu {0} wynosi {1}", pivot.name, rTransform.eulerAngles.z));
        if (rTransform.eulerAngles.z > 180)
        {
            pivot.GetComponent<KoloCzasu_PrzelacznikStron>().WlaczPrawy();
            //pivot.GetComponentInChildren<RectTransform>().localScale = new Vector3(-0.2f, 0.2f, 0.2f);
        }else if(rTransform.eulerAngles.z < 180)
        {
            pivot.GetComponent<KoloCzasu_PrzelacznikStron>().WlaczLewy();
            //pivot.GetComponentInChildren<RectTransform>().localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
    }
}
