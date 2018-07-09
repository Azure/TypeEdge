const connection = new signalR.HubConnectionBuilder()
    .withUrl("/visualizerhub")
    .build();
connection.start().catch(err => console.error(err.toString()));

var numToDraw = 32; // Note: needs to be a power of 2
var pause = false;
/*
 *  <div class="col-6">
        <input type="range" min="2" max="10" value="5" class="slider" id="displayNum">
    </div>
    <div class="col-6">
        Refresh Rate: Every <input type="text" id="frameRate" value="1" /> Frames <input type="button" id="frameButton" value="Go!" /> <input type="button" id="pauseButton" value="Pause/Play" /> <input type="button" id="fftButton" value="Render FFT" />
    </div>
 */

var frames = 1;
var frameNum = 0;
var charts = {};
google.charts.load('current', { 'packages': ['corechart'] });


// This pseudo-object is possibly the most important in the file. It contains 
// header information, all data points, a function to return all points (getLog)
// and functions to get the top Ten or top 100 points so far. Archer has mentioned
// storing this in a database instead, which would help reduce space concerns.
class Chart {    
    updatePoints(newPoints) {
        if (this.append === true) {
            this.points = newPoints.concat(this.points);
        }
        else {
            this.points = newPoints;
        }
        this.points = this.points.slice(0, numToDraw);
    }
    constructor(chart) {
        
        this.chartName = chart.chartName;
        this.xlabel = chart.xlabel;
        this.ylabel = chart.ylabel;
        this.headers = chart.headers;
        this.points = chart.points;
        this.append = chart.append;
        this.htmlChart = document.createElement("div");
        this.htmlChart.id = this.chartName;
        this.htmlChart.style = "width: 900px; height: 500px";
        var chartElement = document.getElementById("charts");
        chartElement.appendChild(this.htmlChart);
        
        this.googleChart = new google.visualization.LineChart(document.getElementById(this.chartName));
        this.options = {
            title: this.chartName, hAxis: { title: this.xlabel }, vAxis: { title: this.ylabel }, curveType: 'function', legend: { position: 'bottom' }
        };
        this.FFTChart = document.createElement("div");
        this.FFTChart.id = this.chartName + "FFT";
        this.FFTChart.style = "width: 900px; height: 500px";
        chartElement.appendChild(this.FFTChart);
        this.FFTOptions = {
            title: 'FFT Chart', hAxis: { title: 'Frequency' }, vAxis: { title: 'Amplitude' }, curveType: 'function', legend: { position: 'bottom' }
        };
        this.FFTGoogleChart = new google.visualization.LineChart(document.getElementById(this.chartName + "FFT"));

    }
}

function getTopNums(chart) {
    var tempArray2 = [];
    chart.points.slice(0, numToDraw).forEach(function (element) {
        tempArray2.push([element[0], element[1]]);
    });
    return tempArray2;
}
function getTop(chart) {
    var tempArray = chart.points.slice(0, numToDraw);
    tempArray.unshift(chart.headers);
    return tempArray;
}

function load() {
    // Called when a new message is received.
    // This method accepts an array of strings for header information and an array of strings for inputs.
    connection.on("ReceiveInput", (obj) => {
        const Msg = JSON.parse(obj);

        // First, check if we have this chart already and it's just an update.

        var messageArray = Msg.messages;
        for (var i = 0; i < messageArray.length; i++) {
            var chart = messageArray[i];
            if (!charts.hasOwnProperty(chart.chartName)) {
                // Does not exist, so let's create it.
                charts[chart.chartName] = new Chart(chart);
            }
            else {
                charts[chart.chartName].updatePoints(chart.points);
            }
        }

        if (!pause && frameNum % frames == 0) { // Draw charts if user wants
            for (var i = 0; i < messageArray.length; i++) {
                drawChart(charts[messageArray[i].chartName]);
            }
        }
        frameNum += 1
    });
}

// Helper function to change the framerate
document.getElementById("frameButton").addEventListener("click", event => {
    var framerate = parseInt(document.getElementById("frameRate").value);
    if (!isNaN(framerate) && framerate != 0) {
        frames = framerate;
    }
});

document.getElementById("pauseButton").addEventListener("click", event => {
    pause = !pause;
});

document.getElementById("fftButton").addEventListener("click", event => {
    drawFFT(charts["Chart1"]);
})

var fft = FFTNayuki(numToDraw);

// Since the number of elements to display must be a power of 2, we use the sliding
// bar to determine the exponent of number to display.
document.getElementById("displayNum").addEventListener("click",
    event => {
        var display = document.getElementById("displayNum");
        numToDraw = Math.pow(2, parseInt(display.value));
        fft = FFTNayuki(numToDraw);
    });

// This actually draws the chart. Possible parameterization: allow the user to determine
// How many elements to pull out.
function drawChart(chart) {
    var top = getTop(chart);
    
    var dataTable = google.visualization.arrayToDataTable(top); // This takes care of the first chart

    chart.googleChart.clearChart();
    chart.googleChart.draw(dataTable, chart.options);
}
function drawFFT(chart) {
    /* Processing for FFT */
    var topNumbers = getTopNums(chart);
    var reals = [];
    var imags = [];

    // Format array correctly and send to FFT. We send 0 for the imaginaries.
    topNumbers.forEach((val, i) => { reals.push(val[1]); imags.push(0); });
    forward(reals, imags);

    // Format array for graph 
    var result = [];
    for (var idx = 1; idx < reals.length; idx++) {
        result.push([topNumbers[idx][0], reals[idx], imags[idx]]);
    }
    // Header
    result.unshift(["Timestamp", "Real", "Imag"]);
    var dataFFTTable = google.visualization.arrayToDataTable(result);

    chart.FFTGoogleChart.clearChart();
    chart.FFTGoogleChart.draw(dataFFTTable, chart.FFTOptions);
}

/* 
 * Free FFT and convolution (JavaScript)
 * 
 * Copyright (c) 2014 Project Nayuki
 * Slightly restructured by Chris Cannam, cannam@all-day-breakfast.com
 */

/* 
 * Construct an object for calculating the discrete Fourier transform (DFT) of size n, where n is a power of 2.
 */
function FFTNayuki(n) {

    this.n = n;
    this.levels = -1;

    for (var i = 0; i < 32; i++) {
        if (1 << i === n) {
            this.levels = i;  // Equal to log2(n)
        }
    }
    if (this.levels === -1) {
        throw "Length is not a power of 2";
    }

    this.cosTable = new Array(n / 2);
    this.sinTable = new Array(n / 2);
    for (i = 0; i < n / 2; i++) {
        this.cosTable[i] = Math.cos(2 * Math.PI * i / n);
        this.sinTable[i] = Math.sin(2 * Math.PI * i / n);
    }

    /* 
     * Computes the discrete Fourier transform (DFT) of the given complex vector, storing the result back into the vector.
     * The vector's length must be equal to the size n that was passed to the object constructor, and this must be a power of 2. Uses the Cooley-Tukey decimation-in-time radix-2 algorithm.
     */
    this.forward = function (real, imag) {

        var n = this.n;

        // Bit-reversed addressing permutation
        for (var i = 0; i < n; i++) {
            var j = reverseBits(i, this.levels);
            if (j > i) {
                var temp = real[i];
                real[i] = real[j];
                real[j] = temp;
                temp = imag[i];
                imag[i] = imag[j];
                imag[j] = temp;
            }
        }

        // Cooley-Tukey decimation-in-time radix-2 FFT
        for (var size = 2; size <= n; size *= 2) {
            var halfsize = size / 2;
            var tablestep = n / size;
            for (i = 0; i < n; i += size) {
                for (j = i, k = 0; j < i + halfsize; j++ , k += tablestep) {
                    var tpre = real[j + halfsize] * this.cosTable[k] +
                        imag[j + halfsize] * this.sinTable[k];
                    var tpim = -real[j + halfsize] * this.sinTable[k] +
                        imag[j + halfsize] * this.cosTable[k];
                    real[j + halfsize] = real[j] - tpre;
                    imag[j + halfsize] = imag[j] - tpim;
                    real[j] += tpre;
                    imag[j] += tpim;
                }
            }
        }

        // Returns the integer whose value is the reverse of the lowest 'bits' bits of the integer 'x'.
        function reverseBits(x, bits) {
            var y = 0;
            for (var i = 0; i < bits; i++) {
                y = (y << 1) | (x & 1);
                x >>>= 1;
            }
            return y;
        }
    }

    /* 
     * Computes the inverse discrete Fourier transform (IDFT) of the given complex vector, storing the result back into the vector.
     * The vector's length must be equal to the size n that was passed to the object constructor, and this must be a power of 2. This is a wrapper function. This transform does not perform scaling, so the inverse is not a true inverse.
     */
    this.inverse = function (real, imag) {
        forward(imag, real);
    }
}