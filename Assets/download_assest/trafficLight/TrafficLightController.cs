using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public Renderer redLightRenderer;
    public Renderer yellowLightRenderer;
    public Renderer greenLightRenderer;

    public Light redLight;
    public Light yellowLight;
    public Light greenLight;

    private void Start()
    {
        StartCoroutine(TrafficLightRoutine());
    }

    private System.Collections.IEnumerator TrafficLightRoutine()
    {
        while (true)
        {
            // 黃燈亮
            SetLightState(false, true, false);
            yield return new WaitForSeconds(3f);

            // 紅燈亮
            SetLightState(true, false, false);
            yield return new WaitForSeconds(5f);

            // 綠燈亮
            SetLightState(false, false, true);
            yield return new WaitForSeconds(10f);
        }
    }

    private void SetLightState(bool redOn, bool yellowOn, bool greenOn)
    {
        // 發光材質 (記得材質要開啟Emission)
        redLightRenderer.material.SetColor("_EmissionColor", redOn ? Color.red : Color.black);
        yellowLightRenderer.material.SetColor("_EmissionColor", yellowOn ? Color.yellow : Color.black);
        greenLightRenderer.material.SetColor("_EmissionColor", greenOn ? Color.green : Color.black);

        // 實體燈光 (可選)
        redLight.enabled = redOn;
        yellowLight.enabled = yellowOn;
        greenLight.enabled = greenOn;
    }
}
