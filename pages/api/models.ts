import { ChatModels } from '@/models';
import type { NextApiRequest, NextApiResponse } from 'next';
import { getServerSession } from 'next-auth';
import { authOptions } from './auth/[...nextauth]';
export const config = {
  api: {
    bodyParser: {
      sizeLimit: '1mb',
    },
  },
  maxDuration: 5,
};

const handler = async (req: NextApiRequest, res: NextApiResponse) => {
  try {
    const session = await getServerSession(req, res, authOptions);
    console.log('session', session);
    const models = await ChatModels.findAll({ where: { enable: true } });
    const _models = models
      .filter((m) => session?.modelIds?.includes(m.modelId))
      .map((x) => {
        return {
          modelId: x.modelId,
          name: x.name,
          type: x.type,
          systemPrompt: x.systemPrompt,
          maxLength: x.maxLength,
          tokenLimit: x.tokenLimit,
          fileSizeLimit: x.fileSizeLimit,
        };
      });
    return res.status(200).json(_models);
  } catch (error) {
    console.error(error);
    return res.status(500).end();
  }
};

export default handler;
