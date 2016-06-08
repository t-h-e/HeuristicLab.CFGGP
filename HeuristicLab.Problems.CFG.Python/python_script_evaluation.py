import json
import threading
import sys

results = ''
exception = ''
stop = [False]


def worker(script):
    global results, stop, exception
    try:
        help_globals = {'stop': stop}
        exec(script, help_globals)
        results = help_globals
    except BaseException as e:
        exc_type, exc_obj, exc_tb = sys.exc_info()
        exception = '{} {} {}'.format(exc_type, exc_obj, e.args)

if __name__ == '__main__':
    while True:
        try:
            message = input()
        except EOFError as err:
            # No data was read with input()
            # HeuristicLab is not running anymore
            break
        
        message_dict = json.loads(message)

        stop[0] = [False]
        t = threading.Thread(target=worker, args=[message_dict['script']])
        t.start()
        t.join(message_dict['timeout'])
        if t.isAlive():
            stop[0] = True
            t.join()
            print(json.dumps({'exception': 'Timeout occurred.'}), flush=True)
        elif exception:
            print(json.dumps({'exception': exception}), flush=True)
            exception = ''
        else:
            ret_message_dict = {}
            for v in message_dict['variables']:
                if v in results:
                    ret_message_dict[v] = results[v]

            print(json.dumps(ret_message_dict), flush=True)

            results = ''
