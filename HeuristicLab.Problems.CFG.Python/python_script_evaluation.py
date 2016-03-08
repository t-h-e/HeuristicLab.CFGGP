import json
import threading
import sys

def worker(script):
    global exception, stop
    try:
        help = {'stop': stop}
        exec(script, help)
        results[0] = help
    except BaseException as e:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        exception = '{} {} {}'.format(exc_type, exc_obj, e.args)


results = [None]
stop = [False]


while True:
    try:
        message = input()
        message_dict = json.loads(message)

        stop[0] = False
        t = threading.Thread(target=worker, args=[message_dict['script']])
        t.start()
        t.join(message_dict['timeout'])
        if t.isAlive():
            stop[0] = True
            t.join()
            print(json.dumps({'exception': 'Timeout occurred.'}))
        elif 'exception' in locals():
            print(json.dumps({'exception': locals()['exception']}))
            del exception
        else:
            ret_message_dict = {}
            for v in message_dict['variables']:
                if v in results[0]:
                    ret_message_dict[v] = results[0][v]

            print(json.dumps(ret_message_dict))
    except BaseException as e:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        print('{} {} {}'.format(exc_type, exc_obj, e.args))