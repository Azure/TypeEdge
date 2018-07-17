const connection = new signalR.HubConnectionBuilder()
    .withUrl("/visualizerhub")
    .build();
connection.start().catch(err => console.error(err.toString()));

var charts = {};


// This pseudo-object is possibly the most important in the file. It contains 
// header information, all data points, a function to return all points (getLog)
// and functions to get the top Ten or top 100 points so far. Archer has mentioned
// storing this in a database instead, which would help reduce space concerns.
class Chart {    
    updatePoints(newPoints) {
        if (this.append === true) {
            for (var i = 0; i < newPoints.length; i++) {
                newPoints[i] = newPoints[i].concat(["null"]);
            }
            this.points = newPoints.concat(this.points);
        }
        else {
            for (var i = 0; i < newPoints.length; i++) {
                newPoints[i] = newPoints[i].concat(["null"]);
            }
            this.points = newPoints;
        }
        this.points = this.points.sort(function (a, b) { return a[0] - b[0] }).reverse().slice(0, this.numToDraw);
        console.log(this.points);
    }
    updateAnomaly(newPoints) {
        if (this.append === true) {
            for (var i = 0; i < newPoints.length; i++) {
                newPoints[i] = newPoints[i].concat(['point { size: 18; shape-type: star; fill-color: #a52714; }']);
            }
            this.points = newPoints.concat(this.points);
        }
        else {
            for (var i = 0; i < newPoints.length; i++) {
                newPoints[i] = newPoints[i].concat(['point { size: 18; shape-type: star; fill-color: #a52714; }']);
            }
            this.points = newPoints;
        }
        this.points = this.points.slice(0, this.numToDraw);
    }
    constructor(chart) {
        this.chartName = chart.chartName;
        this.xlabel = chart.xlabel;
        this.ylabel = chart.ylabel;
        this.headers = chart.headers.concat([{ 'type': 'string', 'role': 'style' }]);
        this.points = [];
        this.append = chart.append;

        /* Set up JavaScript variables */
        this.numToDraw = 32; // Note: needs to be a power of 2
        this.pause = false;
        this.frames = 1;
        this.frameNum = 0;
        var chartElement = document.getElementById("charts");

        /* Now we need to set up the buttons */
        /* First, display */
        var displayNumButton = document.createElement("input");
        displayNumButton.type = "range";
        displayNumButton.min = "2";
        displayNumButton.max = "10";
        displayNumButton.value = "5";
        displayNumButton.class = "slider";
        displayNumButton.id = "displayNum" + this.chartName;
        displayNumButton.name = this.chartName;
        displayNumButton.onclick = function () {
            // Since the number of elements to display must be a power of 2, we use the sliding
            // bar to determine the exponent of number to display.
            var name = "displayNum" + displayNumButton.name;
            var display = document.getElementById(name);
            charts[displayNumButton.name].numToDraw = Math.pow(2, parseInt(display.value));
        };
        chartElement.appendChild(displayNumButton);

        /* Second, framerate */
        var frameButton = document.createElement("input");
        frameButton.type = "text";
        frameButton.id = "frameRate" + this.chartName;
        frameButton.value = "1";
        frameButton.name = this.chartName;
        var description = document.createTextNode(" Frames ");
        chartElement.appendChild(frameButton);
        chartElement.appendChild(description);

        var frameGoButton = document.createElement("input");
        frameGoButton.type = "button";
        frameGoButton.id = "frameButton" + this.chartName;
        frameGoButton.value = "Go!";
        frameGoButton.name = this.chartName;
        frameGoButton.onclick = function () {
            var framerate = parseInt(document.getElementById("frameRate" + frameButton.name).value);
            if (!isNaN(framerate) && framerate != 0) {
                charts[frameButton.name].frames = framerate;
            }
        };
        chartElement.append(frameGoButton);

        /* End Framerate */

        /* Pause Button */

        var pauseButton = document.createElement("input");
        pauseButton.type = "button";
        pauseButton.id = "pauseButton" + this.chartName;
        pauseButton.value = "Pause/Play";
        pauseButton.name = this.chartName;
        pauseButton.onclick = function () {
            charts[pauseButton.name].pause = !charts[pauseButton.name].pause;
        }
        chartElement.append(pauseButton);

        /* End Pause Button */

        /* Render FFT Button */

        var fftButton = document.createElement("input");
        fftButton.type = "button";
        fftButton.id = "fftButton" + this.chartName;
        fftButton.value = "Render FFT";
        fftButton.name = this.chartName;
        fftButton.onclick = function () {
            drawFFT(charts[fftButton.name]);
        }
        chartElement.append(fftButton);

        /* End FFT Button */



        /* Create charts */
        this.htmlChart = document.createElement("div");
        this.htmlChart.id = this.chartName;
        this.htmlChart.style = "width: 1400px; height: 500px";
        this.FFTChart = document.createElement("div");
        this.FFTChart.id = this.chartName + "FFT";
        this.FFTChart.style = "width: 1400px; height: 500px";

        chartElement.appendChild(this.htmlChart);
        chartElement.appendChild(this.FFTChart);

        this.options = {
            title: this.chartName, hAxis: { title: this.xlabel }, vAxis: { title: this.ylabel }, curveType: 'function', legend: { position: 'bottom' }, pointSize: 1
        };
        this.FFTOptions = {
            title: 'FFT Chart', hAxis: { title: 'Frequency' }, vAxis: { title: 'Amplitude' }, curveType: 'function', legend: { position: 'bottom' }
        };
        //console.log(document.getElementById(this.chartName));
        this.googleChart = new google.visualization.LineChart(document.getElementById(this.chartName));
        this.FFTGoogleChart = new google.visualization.LineChart(document.getElementById(this.chartName + "FFT"));
        /* End creation of charts */
    }
}

function getTopNums(chart) {
    var tempArray2 = [];
    chart.points.slice(0, chart.numToDraw).forEach(function (element) {
        tempArray2.push([element[0], element[1]]);
    });
    return tempArray2;
}
function getTop(chart) {
    var tempArray = chart.points.slice(0, chart.numToDraw);
    tempArray.unshift(chart.headers);
    return tempArray;
}

function load() {
    google.charts.load('current', { 'packages': ['corechart'] });
    google.charts.setOnLoadCallback(function () {
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
                
                if (chart.anomaly) {
                    charts[chart.chartName].updateAnomaly(chart.points);
                }
                else {
                    charts[chart.chartName].updatePoints(chart.points);
                }

                // Draw charts if user wants
                chart = charts[chart.chartName];
                if (!chart.pause && chart.frameNum % chart.frames == 0) {
                    drawChart(chart);
                }
                chart.frameNum += 1;
            }
        });
    });
    
}



// This actually draws the chart. Possible parameterization: allow the user to determine
// How many elements to pull out.
function drawChart(chart) {
    var top = getTop(chart);
    //console.log(top);
    var dataTable = google.visualization.arrayToDataTable(top); // This takes care of the first chart

    /* Clear the chart, releasing resources */
    chart.googleChart.clearChart();
    chart.googleChart.hv = {};
    chart.googleChart.iv = {};
    chart.googleChart.jv = {};

    /* With resources cleared, redraw */
    chart.googleChart.draw(dataTable, chart.options);
}
function drawFFT(chart) {
    var fft = FFTNayuki(chart.numToDraw);
    
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

    /* Clear the chart, releasing resources */
    chart.FFTGoogleChart.clearChart();
    chart.FFTGoogleChart.hv = {};
    chart.FFTGoogleChart.iv = {};
    chart.FFTGoogleChart.jv = {};

    /* With resources cleared, redraw */
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