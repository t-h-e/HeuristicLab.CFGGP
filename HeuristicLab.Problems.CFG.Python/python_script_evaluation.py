import json
import threading
import sys
import queue

#results = {}
exception = ['']
stop = [False]

consume = queue.Queue()
produced = queue.Queue()

def worker():
    global stop, exception
    while True:
        script = consume.get()
        if script == None:
            break
        else:
            try:
                help_globals = {'stop': stop}
                exec(script, help_globals)
                produced.put(help_globals)
                #results = help_globals
            except BaseException as e:
                exc_type, exc_obj, exc_tb = sys.exc_info()
                exception[0] = '{} {} {}'.format(exc_type, exc_obj, e.args)
                produced.put(help_globals)
        consume.task_done()



if __name__ == '__main__':
    t = threading.Thread(target=worker)
    t.start()
    while True:
        try:
            message = input()
        except EOFError as err:
            # No data was read with input()
            # HeuristicLab is not running anymore
            # stop thread
            consume.put(None)
            t.join(5)
            break

        message_dict = json.loads(message)
        stop[0] = [False]
        consume.put(message_dict['script'])
        try:
            results = produced.get(message_dict['timeout'])
        except queue.Empty:
            results = None
        # t.join(message_dict['timeout'])
        if not results:
            stop[0] = True
            produced.get()
            # t.join()
            print(json.dumps({'exception': 'Timeout occurred.'}), flush=True)
        elif exception[0]:
            print(json.dumps({'exception': exception[0]}), flush=True)
        else:
            ret_message_dict = {}
            for v in message_dict['variables']:
                if v in results:
                    ret_message_dict[v] = results[v]

            print(json.dumps(ret_message_dict), flush=True)

        # results = {}
        exception[0] = ''
