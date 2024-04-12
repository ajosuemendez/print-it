import http from 'k6/http';
import { sleep } from 'k6';
import { check } from 'k6';

// Read the file data in the global scope
const fileData = open('test.pdf', 'b');

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    vus: 1,
    duration: '10s',
};

export default function () {

    const formData = {
        PrinterPath: 'Brother Color Leg Type1 Class Driver',
        // File: http.file(fileData, 'test.pdf')
        PdfFile: http.file(fileData, 'test.pdf'),
        DocumentName: "test.pdf",
        PrintToFile: true
    };

    // const response = http.post('http://localhost:7000/print/pipe', formData);
    const response = http.post('http://localhost:7000/print/from-pdf', formData);


    // Check if the request was successful
    check(response, {
        'Status is 200': (r) => r.status === 200,
    });

    sleep(1);
}
