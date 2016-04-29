import json
import threading
import sys
import multiprocessing as mp

import time

results = {}
stop = {}
exception = {}

printLock = threading.Lock() # might not be needed


def worker(msg_id, script):
    global results, stop, exception
    try:
        help_globals = {'stop': stop[msg_id]}
        exec(script, help_globals)
        results[msg_id] = help_globals
    except BaseException as e:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        exception[msg_id] = '{} {} {}'.format(exc_type, exc_obj, e.args)


def handle_multicore(message_dict):
    global results, stop, exception
    msg_id = message_dict['id']
    stop[msg_id] = [False]
    t = threading.Thread(target=worker, args=[msg_id, message_dict['script']])
    t.start()
    t.join(message_dict['timeout'])
    if t.isAlive():
        stop[msg_id][0] = True
        t.join()
        del results[msg_id]
        with printLock:
            print(json.dumps({'id' : msg_id, 'exception': 'Timeout occurred.'}), flush=True)
    elif msg_id in exception:
        with printLock:
            print(json.dumps({'id' : msg_id, 'exception': exception[msg_id]}), flush=True)
        del exception[msg_id]
    else:
        ret_message_dict = {'id' : msg_id}
        for v in message_dict['variables']:
            if v in results[msg_id]:
                ret_message_dict[v] = results[msg_id][v]

        with printLock:
            print(json.dumps(ret_message_dict), flush=True)

        del results[msg_id]

    del stop[msg_id]


if __name__ == '__main__':
    p = None
    while True:
        try:
            message = input()
            message_dict = json.loads(message)
            if not p:
                p = mp.Pool()
                # added sleep for one second for windows
                # see http://stackoverflow.com/questions/36919678/python-multiprocessing-pool-does-not-start-right-away/36931537#36931537
                # and http://bugs.python.org/issue26883
                time.sleep(1)
            p.apply_async(handle_multicore, (message_dict,))
        except EOFError as err:
            # No data was read with input()
            # HeuristicLab is not running anymore
            p.close()
            p.join()
            break
