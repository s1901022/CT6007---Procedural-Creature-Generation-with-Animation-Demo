using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreatureGenerator : MonoBehaviour {
    [SerializeField]
    GameObject m_creaturePrefab;

    //UI ELEMENTS
    [SerializeField]
    private Slider m_creatureLengthSlider;
    [SerializeField]
    private TextMeshProUGUI m_creatureLengthText;
    [SerializeField]
    private Slider m_creatureSpineSlider;
    [SerializeField]
    private TextMeshProUGUI m_creatureSpineText;
    [SerializeField]
    private Slider m_creatureAccessorySlider;
    [SerializeField]
    private TextMeshProUGUI m_creatureAccessoryText;
    [SerializeField]
    private Slider m_creatureDeformationSlider;
    [SerializeField]
    private TextMeshProUGUI m_creatureDeformationText;
    [SerializeField]
    private Slider m_creatureAccessoryDeformationSlider;
    [SerializeField]
    private TextMeshProUGUI m_creatureAccessoryDeformationText;

    private float timeScale;

    private void Start() {
        timeScale = 0;
    }

    private void Update() {
        TextValues();

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (timeScale == 0) {
                timeScale = 1;
            }
            else { timeScale = 0; }
            Time.timeScale = timeScale;
        }
    }

    public void GenerateNewCreature() {
        GameObject newCreature = Instantiate(m_creaturePrefab, new Vector3(), Quaternion.identity);
        MeshSkeleton ms = newCreature.GetComponent<MeshSkeleton>();
        ms.SetLength((int)m_creatureLengthSlider.value);
        ms.SetSpineVariation(m_creatureSpineSlider.value);
        ms.SetAccessoryLimit((int)m_creatureAccessorySlider.value);
        ms.SetDeformation(m_creatureDeformationSlider.value);
        ms.SetAccessoryDefformation(m_creatureAccessoryDeformationSlider.value);

        //generate the creature
        ms.Initialise();
    }

    private void TextValues() {
        m_creatureLengthText.text = m_creatureLengthSlider.value.ToString();
        m_creatureSpineText.text = m_creatureSpineSlider.value.ToString();
        m_creatureAccessoryText.text = m_creatureAccessorySlider.value.ToString();
        m_creatureDeformationText.text = m_creatureDeformationSlider.value.ToString();
        m_creatureAccessoryDeformationText.text = m_creatureAccessoryDeformationSlider.value.ToString();
    }
}
