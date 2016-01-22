import json
import threading

def worker(script):
    global exception
    try:
        exec(script, globals())  # set globals, otherwise the variables are not available to main thread
    except BaseException as e:
        exception = str(e)


while True:
    message = input()
    message_dict = json.loads(message)

    stop = False
    t = threading.Thread(target=worker, args=[message_dict['script']])
    t.start()
    t.join(6)
    if t.isAlive():
        stop = True
        print(json.dumps({'exception': 'Timeout occurred.'}))
    elif 'exception' in locals():
        print(json.dumps({'exception': locals()['exception']}))
        del exception
    else:
        ret_message_dict = {}
        for v in message_dict['variables']:
            if v in locals():
                ret_message_dict[v] = locals()[v]

        print(json.dumps(ret_message_dict))