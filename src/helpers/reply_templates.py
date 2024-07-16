from json import load

from src.const import ROOT_DIR


class ReplyTemplates(object):
    """ Reply templates from <root>/reply.json for the text response on Telegram. """

    COMMAND_START = None
    COMMAND_LINK = None
    COMMAND_WELCOME = None
    COMMAND_BAN = None
    COMMAND_DELETE = None

    EVENT_ANONYMOUS_MESSAGE_SENT = None
    EVENT_ANONYMOUS_MESSAGE_RECEIVED = None

    ERROR = None

    def __init__(self):
        pass


def load_reply_templates() -> ReplyTemplates:
    """ Load reply templates from <root>/reply.json """

    reply_object = ReplyTemplates()

    # Load project settings.
    with open('{path}/reply.json'.format(path=ROOT_DIR), 'r') as file:
        reply_data = load(file)

    # Set the command responses.
    commands = reply_data['commands']
    for key, value in commands.items():
        setattr(
            reply_object,
            'COMMAND_' + key,
            value,
        )

    # Set the events responses.
    events = reply_data['events']
    for key, value in events.items():
        setattr(
            reply_object,
            'EVENT_' + key,
            value,
        )

    # Set the error response.
    setattr(
        reply_object,
        'ERROR',
        reply_data['ERROR'],
    )

    return reply_object
