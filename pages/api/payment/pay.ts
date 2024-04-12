import { InternalServerError } from '@/utils/error';
import requestIp from 'request-ip';
import { OrdersManager, WxPayManager } from '@/managers';
import { generateOrderTradeNo } from '@/utils/wxpay/utils';
import { apiHandler } from '@/middleware/api-handler';
import { ChatsApiRequest } from '@/types/next-api';
export const config = {
  api: {
    bodyParser: {
      sizeLimit: '1mb',
    },
  },
  maxDuration: 5,
};

const handler = async (req: ChatsApiRequest) => {
  try {
    if (req.method === 'POST') {
      const { amount } = req.body as { amount: number };
      const outTradeNo = generateOrderTradeNo();
      const order = await OrdersManager.createOrder({
        outTradeNo,
        amount,
        createUserId: req.session.userId,
      });
      const ipAddress = requestIp.getClientIp(req) || '127.0.0.1';
      return await WxPayManager.callWxJSApiPay({
        ipAddress,
        orderId: order.id,
        amount,
        openId: req.session.sub!,
        outTradeNo,
      });
    }
  } catch (error: any) {
    throw new InternalServerError(
      JSON.stringify({ message: error?.message, stack: error?.stack })
    );
  }
};

export default apiHandler(handler);
