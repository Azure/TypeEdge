# TypeEdge.AnomalyDetection
High Frequency Unsupervised **Anomaly Detection on the Edge** using  [TypeEdge](https://github.com/paloukari/TypeEdge).


## Prerequisites
The minimum requirements to get started with **TypeEdge.AnomalyDetection** are:
 - The latest [.NET Core SDK](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.301) (version 2.1.301). To find your current version, run 
`dotnet --version`
 -  An [Azure IoT Hub](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-create-through-portal)


## Quickstart

1. Clone this repo:
    
        git clone https://github.com/paloukari/TypeEdge.AnomalyDetection

1. Edit the `IotHubConnectionString` value of the  `/Thermostat.Emulator/appsettings.json` file. You need to use the **iothubowner** connection string from your Azure **IoT Hub**.

1. Build and run the `Thermostat.Emulator` console app. To observe the generated waveform with a Fast Fourier Transformation, visit the visualization URL at http://localhost:5001. This is a visualization web application that runs on the Edge and helps you understand the data stream characteristics. You will something like this:

<p align="center">
  <img width="80%" height="100%" src="images/VisualizationGraphs.PNG" style="max-width:600px">
</p>

## Injecting ad hoc Anomalies

To inject Anomalies to the waveform, run the `Thermostat.ServiceApp` console app, after editing first the `IotHubConnectionString` value inside the `/Thermostat.ServiceApp/appsettings.json` file. Same as before, you need to use the **iothubowner** connection string from your Azure **IoT Hub**.

This is a Service (cloud) side application that sends twin updates and calls direct methods of the IoT Edge application modules.

<p align="center">
  <img width="80%" height="100%" src="images/serviceApp.png" style="max-width:600px">
</p>

When you call the Anomaly Direct method, an ad hoc anomaly value is generated. This anomaly is a Dirac delta function (Impulse), added to the normal waveform.

A Dirac delta distribution is defined as:
    
 
<body class="markdown-body">
    <p><span class="katex-display"><span class="katex"><span class="katex-mathml"><math><semantics><mrow><mi>f</mi><mo>(</mo><msub><mi>t</mi><mn>0</mn></msub><mo>)</mo><mo>=</mo><msubsup><mo>∫</mo><mrow><mo>−</mo><mi mathvariant="normal">∞</mi></mrow><mi mathvariant="normal">∞</mi></msubsup><mtext>&NegativeThinSpace;</mtext><mi>f</mi><mo>(</mo><mi>t</mi><mo>)</mo><mi>δ</mi><mo>(</mo><mi>t</mi><mo>−</mo><msub><mi>t</mi><mn>0</mn></msub><mo>)</mo><mtext>&ThinSpace;</mtext><mi>d</mi><mi>t</mi></mrow><annotation encoding="application/x-tex">f(t_{0})=\int_{-\infty }^{\infty } \! f(t)\delta(t-t_{0}) \, dt</annotation></semantics></math></span><span class="katex-html" aria-hidden="true"><span class="base"><span class="strut" style="height:1em;vertical-align:-0.25em;"></span><span class="mord mathit" style="margin-right:0.10764em;">f</span><span class="mopen">(</span><span class="mord"><span class="mord mathit">t</span><span class="msupsub"><span class="vlist-t vlist-t2"><span class="vlist-r"><span class="vlist" style="height:0.30110799999999993em;"><span style="top:-2.5500000000000003em;margin-left:0em;margin-right:0.05em;"><span class="pstrut" style="height:2.7em;"></span><span class="sizing reset-size6 size3 mtight"><span class="mord mtight"><span class="mord mtight">0</span></span></span></span></span><span class="vlist-s">​</span></span><span class="vlist-r"><span class="vlist" style="height:0.15em;"><span></span></span></span></span></span></span><span class="mclose">)</span><span class="mspace" style="margin-right:0.2777777777777778em;"></span><span class="mrel">=</span><span class="mspace" style="margin-right:0.2777777777777778em;"></span></span><span class="base"><span class="strut" style="height:2.384573em;vertical-align:-0.970281em;"></span><span class="mop"><span class="mop op-symbol large-op" style="margin-right:0.44445em;position:relative;top:-0.0011249999999999316em;">∫</span><span class="msupsub"><span class="vlist-t vlist-t2"><span class="vlist-r"><span class="vlist" style="height:1.414292em;"><span style="top:-1.7880500000000001em;margin-left:-0.44445em;margin-right:0.05em;"><span class="pstrut" style="height:2.7em;"></span><span class="sizing reset-size6 size3 mtight"><span class="mord mtight"><span class="mord mtight">−</span><span class="mord mtight">∞</span></span></span></span><span style="top:-3.8129000000000004em;margin-right:0.05em;"><span class="pstrut" style="height:2.7em;"></span><span class="sizing reset-size6 size3 mtight"><span class="mord mtight"><span class="mord mtight">∞</span></span></span></span></span><span class="vlist-s">​</span></span><span class="vlist-r"><span class="vlist" style="height:0.970281em;"><span></span></span></span></span></span></span><span class="mspace" style="margin-right:0.16666666666666666em;"></span><span class="mspace" style="margin-right:-0.16666666666666666em;"></span><span class="mord mathit" style="margin-right:0.10764em;">f</span><span class="mopen">(</span><span class="mord mathit">t</span><span class="mclose">)</span><span class="mord mathit" style="margin-right:0.03785em;">δ</span><span class="mopen">(</span><span class="mord mathit">t</span><span class="mspace" style="margin-right:0.2222222222222222em;"></span><span class="mbin">−</span><span class="mspace" style="margin-right:0.2222222222222222em;"></span></span><span class="base"><span class="strut" style="height:1em;vertical-align:-0.25em;"></span><span class="mord"><span class="mord mathit">t</span><span class="msupsub"><span class="vlist-t vlist-t2"><span class="vlist-r"><span class="vlist" style="height:0.30110799999999993em;"><span style="top:-2.5500000000000003em;margin-left:0em;margin-right:0.05em;"><span class="pstrut" style="height:2.7em;"></span><span class="sizing reset-size6 size3 mtight"><span class="mord mtight"><span class="mord mtight">0</span></span></span></span></span><span class="vlist-s">​</span></span><span class="vlist-r"><span class="vlist" style="height:0.15em;"><span></span></span></span></span></span></span><span class="mclose">)</span><span class="mspace" style="margin-right:0.16666666666666666em;"></span><span class="mord mathit">d</span><span class="mord mathit">t</span></span></span></span></span></p>

where f(t) is smooth function.

The Fourier transformation of the Dirac delta function is:

$$\hat \delta(\omega)=\frac {1}{\sqrt{2 \pi}}e^{-j \omega t_{0}}$$
which in our case, just a sine wave.

You can observe this anomaly in the real time visualization page:

<p align="center">
  <img width="80%" height="100%" src="images/Anomaly.png" style="max-width:600px">
</p>

>Note: It's interesting to observe the impact that this spike has on the frequency spectrum.


