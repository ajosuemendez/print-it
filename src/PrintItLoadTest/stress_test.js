import http from 'k6/http';
import { sleep } from 'k6';

export let options = {
    insecureSkipTLSVerify: true,
    noConnectionReuse: false,
    stages: [
        { duration: '2m', target: 5},
        { duration: '5m', target: 5},
        { duration: '2m', target: 10},
        { duration: '5m', target: 10},
        { duration: '2m', target: 15},
        { duration: '5m', target: 15},
        { duration: '10m', target: 0},
    ]
};

const API_BASE_URL = 'http://localhost:7000';

export default () => {
    
    http.batch([
        ['GET', `${API_BASE_URL}/printers/list`]
    ]);

    sleep(1);
};